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
            if (!KiemTraDuLieu.KiemTraEmail(collection["pEmail"]))
            {
                ViewBag.ErrorMessage = "Định dạng email không hợp lệ!";
                return View();
            }

            if (!KiemTraDuLieu.KiemTraSDT_VN(collection["pSDT"]))
            {
                ViewBag.ErrorMessage = "Số điện thoại không hợp lệ";
                return View();
            }

            if (collection["pPassword"].Length < 8)
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
            string namSinh = collection["pYear"] + "-" + collection["pMonth"] + "-" + collection["pDay"];
            string email = collection["pEmail"];
            string sdt = collection["pSDT"];
            string cccd = collection["pCCCD"];
            string diaChi = collection["pDiaChi"];
            string matKhau = collection["pPassword"];


            // Gọi stored procedure mà không sử dụng SqlParameter
            string sql = string.Format(
             "EXEC DangKy N'{0}', '{1}', '{2}', '{3}', '{4}', N'{5}', '{6}'",
             tenKhach, namSinh, email, sdt, cccd, diaChi, matKhau
            );
            try
            {
                db.ExecuteCommand(sql);
            }
            catch (Exception ex)
            {

                ViewBag.ErrorMessage = string.Format("Email, số điện thoại, hoặc CCCD đã được đăng ký. {0}", ex);
                return View();
            }


            // Nếu thành công, chuyển hướng đến trang đăng nhập hoặc thông báo thành công
            TempData["Message"] = "Đăng ký thành công!";
            return RedirectToAction("DangNhap", "TaiKhoan");  // Ví dụ chuyển hướng đến trang đăng nhập          
        }


        public ActionResult QuenMatKhau()
        {
            return View();
        }

        private string GenerateRandomPassword(int length)
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            Random random = new Random();
            return new string(Enumerable.Repeat(validChars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }


        [HttpPost]
        public ActionResult QuenMatKhau(FormCollection form)
        {
            string email = form["email"];
            string phone = form["phone"];
            string cccd = form["cccd"];

            if (IsValidUser(email, phone, cccd))
            {
                string newPass = GenerateRandomPassword(8);

                TaiKhoan tk = db.TaiKhoans.FirstOrDefault(t => t.Email == email);
                KhachHang kh = db.KhachHangs.FirstOrDefault(t => t.Email == email);

                tk.MatKhau = newPass;
                try
                {
                    db.SubmitChanges();

                    EmailSender emailSender = new EmailSender("smtp.gmail.com", 587, true, "dasnn2004@gmail.com", "ckfq pqmc xkll tgcw");
                    emailSender.SendEmail(email, "Đặt lại Mật Khẩu", getMailBody(kh.TenKhach,newPass), true);

                    TempData["Message"] = "Mật khẩu mới đã được gửi về email của bạn thành công! Vui lòng kiểm tra email.";

                    return RedirectToAction("QuenMatKhau");

                }
                catch
                {
                    // Xử lý lỗi khi không thể lưu vào cơ sở dữ liệu
                    TempData["ErrorMessage"] = "Có lỗi xảy ra trong quá trình reset mật khẩu. Vui lòng thử lại sau.";
                    // Log lỗi hoặc xử lý phù hợp
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Thông tin không chính xác. Vui lòng thử lại.";
            }
            return RedirectToAction("QuenMatKhau");

        }

        private bool IsValidUser(string email, string phone, string cccd)
        {
            TaiKhoan tk = db.TaiKhoans.FirstOrDefault(t => t.Email == email);
            if (tk != null)
            {
                KhachHang kh = db.KhachHangs.FirstOrDefault(k => k.SDT == phone && k.CCCD == cccd);

                if (kh != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }


        private string getMailBody(string userName, string tempPassword)
        {
            return string.Format(@"
    <html>
    <head>
        <style>
            body {{
                font-family: Arial, sans-serif;
                background-color: #f4f4f4;
                margin: 0;
                padding: 0;
            }}
            .container {{
                max-width: 600px;
                margin: 20px auto;
                background-color: #ffffff;
                padding: 20px;
                border-radius: 8px;
                box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
            }}
            .header {{
                text-align: center;
                background-color: #4CAF50;
                padding: 10px;
                color: white;
                font-size: 24px;
                border-top-left-radius: 8px;
                border-top-right-radius: 8px;
            }}
            .content {{
                padding: 20px;
                text-align: left;
                line-height: 1.6;
            }}
            .content p {{
                margin: 0;
                padding-bottom: 10px;
            }}
            .content .highlight {{
                color: #4CAF50;
                font-weight: bold;
            }}
            .footer {{
                text-align: center;
                padding: 10px;
                font-size: 12px;
                color: #666666;
            }}
            .button {{
                display: inline-block;
                padding: 10px 20px;
                font-size: 16px;
                color: white;
                background-color: #4CAF50;
                text-decoration: none;
                border-radius: 5px;
            }}
            .button:hover {{
                background-color: #45a049;
            }}
        </style>
    </head>
    <body>
        <div class='container'>
            <div class='header'>
                Hệ thống Quản lý Vé Tàu
            </div>
            <div class='content'>
                <p>Xin chào <strong>{0}</strong>,</p>
                <p>Chúng tôi đã nhận được yêu cầu đặt lại mật khẩu của bạn. Mật khẩu tạm thời của bạn là:</p>
                <p class='highlight'>{1}</p>
                <p>Vì lý do bảo mật, vui lòng thay đổi mật khẩu ngay sau khi đăng nhập.</p>
                <p>Nếu bạn không yêu cầu thay đổi này, xin vui lòng liên hệ với chúng tôi ngay lập tức.</p>
                <p>Trân trọng,</p>
                <p>Đội ngũ hỗ trợ</p>
            </div>
            <div class='footer'>
                &copy; 2024 Hệ thống Quản lý Vé Tàu. Mọi quyền được bảo lưu.
            </div>
        </div>
    </body>
    </html>", userName, tempPassword);
        }


    }
}