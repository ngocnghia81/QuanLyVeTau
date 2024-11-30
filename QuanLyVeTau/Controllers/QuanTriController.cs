using Newtonsoft.Json;
using PagedList;
using QuanLyVeTau.Models;
using QuanLyVeTau.Tools;
using System;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;

namespace QuanLyVeTau.Controllers
{
    public class QuanTriController : Controller
    {

        private readonly QuanLyVeTauDBDataContext db;

        public QuanTriController()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["QL_VETAUConnectionString"].ConnectionString;
            db = new QuanLyVeTauDBDataContext(connectionString);
        }

        public ActionResult DashBoard()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("DangNhap", "QuanTri");
            }

            var tongNguoiDung = db.Vw_TongNguoiDungs.FirstOrDefault().TongNguoiDung;
            var tongVeDaBan = db.Vw_TongVeDaBans.FirstOrDefault().TongVeDaBan;
            var tongDoanhThu = db.Vw_TongDoanhThus.FirstOrDefault().TongDoanhThu;
            var tongPhanHoi = db.Vw_TongPhanHois.FirstOrDefault().TongPhanHoi;

            var doanhThuTheoThang = db.Vw_DoanhThuTheoThangs
                                      .OrderBy(dt => dt.Nam)   
                                      .ThenBy(dt => dt.Thang) 
                                      .ToList().Take(5);
            var soKhachHang = db.Vw_SoKhachHangs.FirstOrDefault().SoKhachHang;
            var soKhuyenMai = db.Vw_SoKhuyenMais.FirstOrDefault().SoKhuyenMai;
            var soVeTheoThang = db.Vw_SoVeTheoThangs.ToList();
            var danhThuThucNhan = db.Vw_DoanhThuThucNhans.FirstOrDefault().DoanhThuThucNhan;

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
                Session["Email"] = taiKhoan.Email;
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