using QuanLyVeTau.Models;
using QuanLyVeTau.Tools;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;


namespace QuanLyVeTau.Controllers
{
    public class TaiKhoanController : Controller
    {
        private QuanLyVeTauDBDataContext db;

        public TaiKhoanController()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["QL_VETAUConnectionString1"].ConnectionString;
            db = new QuanLyVeTauDBDataContext(connectionString);
        }

        // GET: TaiKhoan
        public ActionResult Index()
        {
            return View();
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

            var taiKhoan = db.TaiKhoans.SingleOrDefault(u => u.Email == username && u.MatKhau == password) ?? null;
            if (taiKhoan != null)
            {
                FormsAuthentication.SetAuthCookie(taiKhoan.Email, false);
                return RedirectToAction("Index", "Ve"); 
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

        public ActionResult DangKy()
        {
    
            return View();
        }
        [HttpPost]
        public ActionResult DangKy(FormCollection collection)
        {
            if (KiemTraDuLieu.KiemTraEmail(collection["pEmail"]))
            {
                ViewBag.ErrorMessage = "Định dạng email không hợp lệ!";
                return View();
            }

            if (KiemTraDuLieu.KiemTraSDT_VN(collection["pSDT"]))
            {
                ViewBag.ErrorMessage = "Số điện thoại không hợp lệ";
                return View();
            }

            if (collection["pPassword"].Length <8)
            {
                ViewBag.ErrorMessage = "Mật khẩu tối thiểu 8 kí tự";
                return View();
            }

            if (collection["pPassword"] != collection["pConfirmPassword"])
            {
                ViewBag.ErrorMessage = "Mật khẩu không trùng khớp";
                return View();
            }


            string tenKhach = collection["pTen"];
            string namSinh = collection["pYear"] + collection["pMonth"] + collection["pDay"];
            string email = collection["pEmail"];
            string sdt = collection["pSDT"];
            string cccd = collection["pCCCD"];
            string diaChi = collection["pDiaChi"];
            string matKhau = collection["pPassword"];

            // Gọi stored procedure mà không sử dụng SqlParameter
            string sql = string.Format(
             "EXEC DangKy '{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}'",
             tenKhach, namSinh, email, sdt, cccd, diaChi, matKhau
            );
            db.ExecuteCommand(sql);


            // Nếu thành công, chuyển hướng đến trang đăng nhập hoặc thông báo thành công
            ViewBag.Message = "Đăng ký thành công!";
            return RedirectToAction("DangNhap", "TaiKhoan");  // Ví dụ chuyển hướng đến trang đăng nhập          
        }
    }
}