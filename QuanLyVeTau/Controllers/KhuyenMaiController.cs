using PagedList;
using QuanLyVeTau.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;

namespace QuanLyVeTau.Controllers
{
    public class KhuyenMaiController : Controller
    {

        private readonly QuanLyVeTauDBDataContext db;

        public KhuyenMaiController()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["QL_VETAUConnectionString4"].ConnectionString;
            db = new QuanLyVeTauDBDataContext(connectionString);
        }


        public ActionResult DanhSachKhuyenMai(string search = "", DateTime? ngayBatDau = null, DateTime? ngayKetThuc = null, int page = 1)
        {
            // Truy vấn khuyến mãi từ database
            var khuyenMais = db.KhuyenMais.AsQueryable();

            // Lọc theo tên khuyến mãi
            if (!string.IsNullOrEmpty(search))
            {
                khuyenMais = khuyenMais.Where(km => km.TenKhuyenMai.ToLower().Contains(search.ToLower()) ||
                                                km.MaKhuyenMai.ToLower().Contains(search.ToLower()));
            }

            // Lọc theo ngày bắt đầu
            if (ngayBatDau.HasValue)
            {
                khuyenMais = khuyenMais.Where(km => km.NgayBatDau >= ngayBatDau.Value);
            }

            // Lọc theo ngày kết thúc
            if (ngayKetThuc.HasValue)
            {
                khuyenMais = khuyenMais.Where(km => km.NgayKetThuc <= ngayKetThuc.Value);
            }

            // Sắp xếp kết quả và phân trang
            int pageSize = 12;
            var result = khuyenMais.OrderBy(km => km.NgayBatDau) // Hoặc theo một trường khác nếu cần
                                   .ToPagedList(page, pageSize);

            // Truyền kết quả tới View
            return View(result);
        }


        public ActionResult TaoKhuyenMai()
        {
            return View();
        }

        [HttpPost]
        public ActionResult TaoKhuyenMai(FormCollection form)
        {
            try
            {
                var khuyenMai = new KhuyenMai
                {
                    TenKhuyenMai = form["TenKhuyenMai"],
                    PhanTramGiam = Double.Parse(form["PhanTramGiam"]),
                    SoTienGiamToiDa = int.Parse(form["SoTienGiamToiDa"]),
                    NgayBatDau = DateTime.Parse(form["NgayBatDau"]),
                    NgayKetThuc = DateTime.Parse(form["NgayKetThuc"]),
                    SoLuong = Convert.ToInt32(form["SoLuong"]),
                    SoLuongConLai = Convert.ToInt32(form["SoLuong"]), 
                };

                db.KhuyenMais.InsertOnSubmit(khuyenMai);
                db.SubmitChanges();

                return RedirectToAction("DanhSachKhuyenMai");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi thêm khuyến mãi: " + ex.Message);
            }

            return View();
        }


        [HttpGet]
        public ActionResult ChinhSuaKhuyenMai(string id)
        {
            var khuyenMai = db.KhuyenMais.FirstOrDefault(km => km.MaKhuyenMai == id);

            if (khuyenMai == null)
            {
                return HttpNotFound();
            }

            return View(khuyenMai);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChinhSuaKhuyenMai(string id, FormCollection collection)
        {
            var khuyenMai = db.KhuyenMais.FirstOrDefault(km => km.MaKhuyenMai == id);

            if (khuyenMai != null)
            {
                // Cập nhật số lượng
                int soLuongMoi = int.Parse(collection["SoLuong"]);
                khuyenMai.SoLuong += soLuongMoi;

                // Cập nhật số lượng còn lại, cộng với số lượng mới
                khuyenMai.SoLuongConLai = khuyenMai.SoLuongConLai + soLuongMoi;

                // Cập nhật tên khuyến mãi
                khuyenMai.TenKhuyenMai = collection["TenKhuyenMai"];

                // Cập nhật phần trăm giảm
                khuyenMai.PhanTramGiam = Double.Parse(collection["PhanTramGiam"]);

                // Cập nhật số tiền giảm tối đa
                khuyenMai.SoTienGiamToiDa = int.Parse(collection["SoTienGiamToiDa"]);

                // Cập nhật ngày bắt đầu và kết thúc
                khuyenMai.NgayBatDau = DateTime.Parse(collection["NgayBatDau"]);
                khuyenMai.NgayKetThuc = DateTime.Parse(collection["NgayKetThuc"]);

                // Lưu thay đổi vào cơ sở dữ liệu
                db.SubmitChanges();

                // Chuyển hướng đến trang quản lý khuyến mãi
                return RedirectToAction("DanhSachKhuyenMai");
            }

            return HttpNotFound();
        }



        public ActionResult XemHoaDon(string maKhuyenMai, int page = 1)
        {
            // Lấy danh sách hóa đơn liên quan đến mã khuyến mãi
            var hoaDons = db.HoaDons
                .Where(hd => hd.MaKhuyenMai == maKhuyenMai)
                .Select(hd => new HoaDonViewModel
                {
                    HoaDon = hd,
                    KhuyenMai = hd.KhuyenMai,
                })
                .ToList();

            // Kiểm tra xem có hóa đơn không
            if (hoaDons == null || !hoaDons.Any())
            {
                ViewBag.Message = "Không có hóa đơn nào cho khuyến mãi này.";
            }

            // Số lượng bản ghi hiển thị trên mỗi trang
            int pageSize = 12;
            var result = hoaDons
                .OrderBy(hd => hd.HoaDon.ThoiGianLapHoaDon)  // Sắp xếp theo thời gian lập hóa đơn
                .ToPagedList(page, pageSize);  // Phân trang

            return View(result);

        }



    }
}