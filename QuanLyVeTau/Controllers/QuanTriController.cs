using Newtonsoft.Json;
using PagedList;
using QuanLyVeTau.Models;
using QuanLyVeTau.Tools;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Data.Linq;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI;
using WebGrease.Css.Extensions;

namespace QuanLyVeTau.Controllers
{
    public class QuanTriController : Controller
    {

        private readonly QuanLyVeTauDBDataContext db;

        public QuanTriController()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["QL_VETAUConnectionString1"].ConnectionString;
            db = new QuanLyVeTauDBDataContext(connectionString);
        }

        public ActionResult DashBoard()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("DangNhap", "QuanTri");
            }

            var tongNguoiDung = db.TaiKhoans.Count();

            var tongVeDaBan = db.Ves
                                .Where(v => v.DaThuHoi == false &&
                                            !db.LichSuDoiTraVes.Any(ls => ls.MaVe == v.MaVe && ls.HanhDong == "Trả"))
                                .Count();

            var tongDoanhThu = (db.HoaDons.Sum(hd => (decimal?)hd.ThanhTien) ?? 0) +
                                (db.LichSuDoiTraVes.Sum(ls => (decimal?)ls.LePhi) ?? 0);

            var tongPhanHoi = db.PhanHois.Count();

            var doanhThuTheoThang = db.HoaDons
                                      .GroupBy(hd => new { hd.ThoiGianLapHoaDon.Value.Year, hd.ThoiGianLapHoaDon.Value.Month })
                                      .Select(g => new
                                      {
                                          Thang = g.Key.Month,
                                          Nam = g.Key.Year,
                                          TongDoanhThu = g.Sum(h => h.ThanhTien)
                                      })
                                      .OrderBy(d => d.Nam).ThenBy(d => d.Thang)
                                      .ToList();

            var soKhachHang = db.KhachHangs.Count();

            var soKhuyenMai = db.KhuyenMais.Count();

            var soVeTheoThang = db.Ves
                    .Where(v => v.DaThuHoi == false &&
                                !db.LichSuDoiTraVes.Any(ls => ls.MaVe == v.MaVe && ls.HanhDong == "Trả") &&
                                v.HoaDon.ThoiGianLapHoaDon.HasValue)
                    .GroupBy(v => new
                    {
                        Year = v.HoaDon.ThoiGianLapHoaDon.Value.Year,
                        Month = v.HoaDon.ThoiGianLapHoaDon.Value.Month
                    })
                    .Select(g => new
                    {
                        Thang = g.Key.Month,
                        Nam = g.Key.Year,
                        SoVeBan = g.Count()
                    })
                    .OrderBy(g => g.Nam).ThenBy(g => g.Thang)  // Optional: To ensure it's ordered by year and month
                    .ToList();

            var danhThuThucNhan = (db.HoaDons
                                    .Where(hd => hd.Ves
                                        .Any(ve => ve.ChiTietLichTrinh.LichTrinhTau.TrangThai.ToString().ToLower() == "hoàn thành"))
                                    .Sum(hd => (decimal?)hd.ThanhTien) ?? 0) +
                                    (db.LichSuDoiTraVes.Sum(ls => (decimal?)ls.LePhi) ?? 0);

            ViewBag.tongNguoiDung = tongNguoiDung;
            ViewBag.tongVeDaBan = tongVeDaBan;
            ViewBag.tongDoanhThu = tongDoanhThu;
            ViewBag.tongPhanHoi = tongPhanHoi;
            ViewBag.danhThuThucNhan = danhThuThucNhan;
            ViewBag.donThuTheoThang = JsonConvert.SerializeObject(doanhThuTheoThang);
            ViewBag.soVeTheoThang = JsonConvert.SerializeObject(soVeTheoThang);
            ViewBag.soKhachHang = soKhachHang;
            ViewBag.soKhuyenMai = soKhuyenMai;

            ViewBag.ActiveLink = "dashboardLink";

            return View("DashBoard");
        }


        public ActionResult DangNhap()
        {
            return View("DangNhap");
        }

        [HttpPost]
        public ActionResult DangNhap(FormCollection collection)
        {
            string username = collection["pUsername"];
            string password = collection["pPassword"];

            var taiKhoan = db.TaiKhoanNhanViens.SingleOrDefault(u => u.Email == username && u.MatKhau == password) ?? null;

            if (taiKhoan != null)
            {
                FormsAuthentication.SetAuthCookie(taiKhoan.Email, false);
                TempData["Email"] = taiKhoan.Email;
                return RedirectToAction("DashBoard");
            }
            else if (!KiemTraDuLieu.KiemTraEmail(username))
            {
                ViewBag.ErrorMessage = "Email không đúng định dạng!";
                return View();
            }
            else
            {
                ViewBag.ErrorMessage = "Tên đăng nhập hoặc mật khẩu không đúng!";
                return View();
            }
        }

        public ActionResult DangXuat()
        {
            FormsAuthentication.SignOut();

            return RedirectToAction("Index", "Ve");
        }



        public ActionResult QuanLyVe(bool? daThuHoi, string maTau = "", string maKhach = "", string maVe = "", string diemDi = "", string diemDen = "", int page = 1)
        {
            var ves = db.Ves.AsQueryable();

            // Lọc theo trạng thái thu hồi vé
            if (daThuHoi.HasValue)
            {
                ves = ves.Where(v => v.DaThuHoi == daThuHoi);
            }

            // Lọc theo tàu
            if (!string.IsNullOrEmpty(maTau))
            {
                ves = ves.Where(v => v.Khoang.Toa.Tau.MaTau.ToLower().Contains(maTau));
            }

            // Lọc theo khách hàng
            if (!string.IsNullOrEmpty(maKhach))
            {
                ves = ves.Where(v => v.HoaDon.MaKhach.ToLower().Contains(maKhach.ToLower()));
            }

            // Lọc theo mã vé
            if (!string.IsNullOrEmpty(maVe))
            {
                ves = ves.Where(v => v.MaVe.ToLower().Contains(maVe.ToLower()));
            }

            // Lọc theo điểm đi
            if (!string.IsNullOrEmpty(diemDi))
            {
                ves = ves.Where(v => v.ChiTietLichTrinh.Ga.DiaChi.ToLower().Contains(diemDi.ToLower()));
            }

            // Lọc theo điểm đến
            if (!string.IsNullOrEmpty(diemDen))
            {
                ves = ves.Where(v => v.ChiTietLichTrinh1.Ga.DiaChi.ToLower().Contains(diemDen.ToLower()));
            }

            // Tạo ViewBag cho liên kết của menu
            ViewBag.ActiveLink = "manageTicketsLink";

            // Phân trang
            int pageSize = 12;  // Số vé hiển thị trên mỗi trang
            var result = ves.ToPagedList(page, pageSize);  // Phân trang danh sách vé

            // Trả về view với danh sách vé đã phân trang
            return View(result);  // Trả về IPagedList<Ve> cho View
        }



        public ActionResult ChiTietVe(string maVe)
        {
            var ve = db.Ves
                       .FirstOrDefault(v => v.MaVe == maVe);

            if (ve == null)
            {
                return HttpNotFound();
            }

            var hoaDon = db.HoaDons
                           .FirstOrDefault(hd => hd.MaHoaDon == ve.MaHoaDon);

            var khachHang = db.KhachHangs
                              .FirstOrDefault(kh => kh.MaKhach == hoaDon.MaKhach);

            var khoang = db.Khoangs.FirstOrDefault(k => k.MaKhoang == ve.MaKhoang);

            var toa = db.Toas
                        .FirstOrDefault(t => t.MaToa == khoang.MaToa);

            var nhatKt = db.NhatKyTaus.FirstOrDefault(nk => nk.MaNhatKy == ve.MaNhatKy);

            var lichTrinh = db.LichTrinhTaus
                              .FirstOrDefault(lt => lt.MaLichTrinh == nhatKt.MaLichTrinh);

            var chiTietLichTrinhDi = db.ChiTietLichTrinhs
                                       .FirstOrDefault(ct => ct.MaChiTiet == ve.DiemDi);

            var chiTietLichTrinhDen = db.ChiTietLichTrinhs
                                        .FirstOrDefault(ct => ct.MaChiTiet == ve.DiemDen);

            var tau = db.Taus
                        .FirstOrDefault(t => t.MaTau == toa.MaTau);

            DateTime tgkh = nhatKt.NgayGio;
            DateTime tgDen;

            try
            {
                tgDen = LayTongThoiGianDiChuyen(nhatKt.MaNhatKy, tgkh, chiTietLichTrinhDi.MaGa, chiTietLichTrinhDen.MaGa);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi tính thời gian đến: " + ex.Message);
                return View("Error");
            }

            double kc = db.ChiTietLichTrinhs
                          .Where(ct => ct.MaLichTrinh == lichTrinh.MaLichTrinh &&
                                       ct.Stt_Ga >= chiTietLichTrinhDi.Stt_Ga &&
                                       ct.Stt_Ga <= chiTietLichTrinhDen.Stt_Ga)
                          .Sum(ct => ct.KhoangCachTuTramTruoc);

            var veChiTiet = new VeChiTietViewModel

            {
                MaVe = ve.MaVe,
                GiaVe = ve.GiaVe,
                TenKhachHang = khachHang?.TenKhach,
                TenTau = tau?.TenTau,
                TenLoaiToa = toa?.MaLoaiToa,
                TenLichTrinh = lichTrinh?.TenLichTrinh,
                DiemDi = chiTietLichTrinhDi?.MaGa,
                DiaChiDiemDi = db.Gas.FirstOrDefault(g => g.MaGa == chiTietLichTrinhDi.MaGa)?.DiaChi,
                DiemDen = chiTietLichTrinhDen?.MaGa,
                DiaChiDiemDen = db.Gas.FirstOrDefault(g => g.MaGa == chiTietLichTrinhDen.MaGa)?.DiaChi,
                ThoiGianKhoiHanh = tgkh,
                ThoiGianDen = tgDen,
                KhoangCach = kc,
                SttGhe = ve.Stt_Ghe,
                ThanhTien = hoaDon?.ThanhTien,
                ThoiGianLapHoaDon = hoaDon?.ThoiGianLapHoaDon,
                DaThuHoi = ve.DaThuHoi
            };

            return View(veChiTiet);
        }



        private DateTime LayTongThoiGianDiChuyen(string maNhatKy, DateTime thoiGianDi, string maGaDi, string maGaDen)
        {
            DataTable dtReturned = new DataTable();
            DateTime thoiGianDiChuyen = DateTime.MinValue; // Khởi tạo giá trị mặc định nếu không có dữ liệu

            // Chuỗi truy vấn để gọi hàm SQL với các tham số truyền vào
            string sql2 = string.Format("SELECT dbo.TinhTongThoiGianDiChuyen('{0}', '{1}', '{2}', '{3}')",
                                        maNhatKy,
                                        thoiGianDi.ToString("yyyy-MM-dd HH:mm:ss"),
                                        maGaDi,
                                        maGaDen);

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["QL_VETAUConnectionString1"].ConnectionString))
            {
                // Mở kết nối đến cơ sở dữ liệu
                connection.Open();

                // Tạo SqlDataAdapter để thực thi truy vấn và điền kết quả vào DataTable
                using (var adapter = new SqlDataAdapter(sql2, connection))
                {
                    // Điền dữ liệu vào DataTable
                    adapter.Fill(dtReturned);
                }
            }

            // Kiểm tra nếu DataTable có ít nhất một hàng kết quả
            if (dtReturned.Rows.Count > 0)
            {
                // Giả sử kết quả trả về chứa thời gian ở cột đầu tiên (cột index 0)
                thoiGianDiChuyen = Convert.ToDateTime(dtReturned.Rows[0][0]);
            }
            else
            {
                throw new Exception("Không tìm thấy dữ liệu thời gian di chuyển.");
            }

            return thoiGianDiChuyen;
        }


        


        public ActionResult QuanLyHoaDon(string maTau = "", string maKhach = "", bool? daChay = null, int page = 1)
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