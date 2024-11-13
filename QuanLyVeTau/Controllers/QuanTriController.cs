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
    }
}