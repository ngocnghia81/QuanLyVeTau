using QuanLyVeTau.Models;
using QuanLyVeTau.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace QuanLyVeTau.Controllers
{
    public class TaiKhoanController : Controller
    {
        private QuanLyVeTauDBDataContext db = new QuanLyVeTauDBDataContext("Data Source=DESKTOP-7TLHHMR;Initial Catalog=QL_VETAU;Integrated Security=True;Encrypt=False");
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

    }
}