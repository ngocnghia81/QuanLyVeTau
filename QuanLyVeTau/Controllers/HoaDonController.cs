using PagedList;
using QuanLyVeTau.Models;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;

namespace QuanLyVeTau.Controllers
{
    public class HoaDonController : Controller
    {
        private readonly QuanLyVeTauDBDataContext db;

        public HoaDonController()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["QL_VETAUConnectionString4"].ConnectionString;
            db = new QuanLyVeTauDBDataContext(connectionString);
        }

        public ActionResult DanhSachHoaDon(string maTau = "", string maKhach = "", bool? daChay = null, int page = 1)
        {
            var hoaDons = db.HoaDons.AsQueryable();

            // Lọc theo mã tàu
            if (!string.IsNullOrEmpty(maTau))
            {
                var nhatKyTaus = db.NhatKyTaus.Where(nk => nk.MaTau.ToLower().Contains(maTau.ToLower())).ToList();
                var chiTietLichTrinhs = db.ChiTietLichTrinhs
                    .Where(ct => nhatKyTaus.Select(nk => nk.MaLichTrinh).Contains(ct.MaLichTrinh))
                    .ToList();
                var veIds = db.Ves
                    .Where(v => chiTietLichTrinhs.Select(ct => ct.MaLichTrinh).Contains(v.ChiTietLichTrinh.MaLichTrinh))
                    .Select(v => v.MaHoaDon)
                    .ToList();
                hoaDons = hoaDons.Where(hd => veIds.Contains(hd.MaHoaDon));
            }

            // Lọc theo mã khách
            if (!string.IsNullOrEmpty(maKhach))
            {
                hoaDons = hoaDons.Where(h => h.MaKhach.ToLower().Contains(maKhach.ToLower()));
            }

            // Lọc theo trạng thái chạy (Hoàn thành hay chưa)
            if (daChay.HasValue)
            {
                // Lọc các NhatKyTaus dựa trên trạng thái
                var nhatKyTauFiltered = db.NhatKyTaus
                    .Where(nk => daChay.Value
                        ? nk.TrangThai.ToLower() == "hoàn thành"
                        : nk.TrangThai.ToLower() == "chưa hoàn thành");

                // Truy xuất các MaNhatKy đã lọc dựa trên trạng thái
                var nhatKyTauIds = nhatKyTauFiltered.Select(nk => nk.MaNhatKy).ToList();

                // Truy vấn để lấy danh sách hoa đơn liên quan đến trạng thái của NhatKyTaus
                hoaDons = hoaDons
                    .Where(hd => db.Ves
                        .Any(v => v.MaHoaDon == hd.MaHoaDon && nhatKyTauIds.Contains(v.MaNhatKy)))
                    .AsQueryable();
            }



            // Lấy dữ liệu chi tiết hóa đơn
            List<HoaDonViewModel> hoaDonViewModels = new List<HoaDonViewModel>();
            foreach (var hoaDon in hoaDons)
            {
                var khachHang = db.KhachHangs.FirstOrDefault(kh => kh.MaKhach == hoaDon.MaKhach);
                var khuyenMai = db.KhuyenMais.FirstOrDefault(km => km.MaKhuyenMai == hoaDon.MaKhuyenMai);
                var ve = db.Ves.FirstOrDefault(v => v.MaHoaDon == hoaDon.MaHoaDon);

                hoaDonViewModels.Add(new HoaDonViewModel
                {
                    HoaDon = hoaDon,
                    Ve = ve,
                    KhuyenMai = khuyenMai
                });
            }

            ViewBag.ActiveLink = "manageInvoices";
            int pageSize = 5;
            var result = hoaDonViewModels.ToPagedList(page, pageSize);

            return View(result);
        }

    }
}