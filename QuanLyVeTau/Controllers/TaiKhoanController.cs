using QuanLyVeTau.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QuanLyVeTau.Controllers
{
    public class TaiKhoanController : Controller
    {
        private QuanLyVeTauDBDataContext db;
        // GET: TaiKhoan
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult DangNhap(FormCollection collection)
        {
            string username = collection["pUsername"];
            string password = collection["pPassword"];

            // Kiểm tra thông tin đăng nhập
            bool isValidUser = CheckUserCredentials(username, password);

            if (isValidUser)
            {
                // Nếu thành công, chuyển hướng tới trang chính hoặc nơi cần thiết
                return RedirectToAction("Index", "Home");
            }
            else
            {
                // Nếu thông tin không hợp lệ, hiển thị thông báo lỗi
                ViewBag.ErrorMessage = "Invalid username or password.";
                return View();
            }
        }
    }
}