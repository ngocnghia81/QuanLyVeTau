using PagedList;
using QuanLyVeTau.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Mvc;
using QuanLyVeTau.Tools;

namespace QuanLyVeTau.Controllers
{
    public class NhanVienController : Controller
    {
        private readonly QuanLyVeTauDBDataContext db;
        private readonly string connectionString;

        public NhanVienController()
        {
            connectionString = ConfigurationManager.ConnectionStrings["QL_VETAUConnectionString"].ConnectionString;
            db = new QuanLyVeTauDBDataContext(connectionString);
        }

        public ActionResult DanhSachNhanVien(string Search = "", string VaiTro = "", string ChucVu = "", int page = 1)
        {
            var NhanViens = db.NhanViens.AsQueryable();

            if (!String.IsNullOrEmpty(Search))
            {
                NhanViens = NhanViens.Where(nv =>
                                nv.MaNhanVien.ToLower().Contains(Search.ToLower()) ||
                                nv.Email.ToLower().Contains(Search.ToLower()) ||
                                nv.SDT.ToLower().Contains(Search.ToLower()) ||
                                nv.CCCD.ToLower().Contains(Search.ToLower()) ||
                                nv.TenNhanVien.ToLower().Contains(Search.ToLower()));
            }

            if (!String.IsNullOrEmpty(VaiTro))
            {
                var emailsWithRole = db.TaiKhoanNhanViens
                                       .Where(tk => tk.VaiTro.ToLower().Contains(VaiTro.ToLower()))
                                       .Select(tk => tk.Email).ToList();

                NhanViens = NhanViens.Where(nv => emailsWithRole.Contains(nv.Email));
            }

            if (!String.IsNullOrEmpty(ChucVu))
            {
                NhanViens = NhanViens.Where(nv => db.ChucVus.Any(cv => cv.TenChucVu.ToLower().Contains(ChucVu.ToLower()) && cv.MaChucVu == nv.MaChucVu));
            }

            var model = NhanViens.OrderBy(nv => nv.MaNhanVien)
                                 .ToList()
                                 .Select(nv => new NhanVienViewModel
                                 {
                                     MaNhanVien = nv.MaNhanVien,
                                     TenNhanVien = nv.TenNhanVien,
                                     Email = nv.Email,
                                     SDT = nv.SDT,
                                     CCCD = nv.CCCD,
                                     NamSinh = nv.NamSinh ?? 0,
                                     HeSoLuong = nv.HeSoLuong.HasValue ? (double)nv.HeSoLuong.Value : 0,
                                     VaiTro = db.TaiKhoanNhanViens.FirstOrDefault(tk => tk.Email == nv.Email)?.VaiTro,
                                     DaXoa = db.TaiKhoanNhanViens.FirstOrDefault(tk => tk.Email == nv.Email)?.DaXoa ?? false,
                                     ChucVu = db.ChucVus.FirstOrDefault(cv => cv.MaChucVu == nv.MaChucVu).TenChucVu,
                                     MoTa = db.ChucVus.FirstOrDefault(cv => cv.MaChucVu == nv.MaChucVu).MoTa
                                 }).ToPagedList(page, 10);

            ViewBag.ChucVus = db.ChucVus.ToList();
            ViewBag.Search = Search;
            ViewBag.VaiTro = VaiTro;
            ViewBag.ChucVu = ChucVu;

            return View(model);
        }


        public ActionResult LayThongTinNhanVien(string MaNhanVien)
        {
            var nv = db.NhanViens.FirstOrDefault(n => n.MaNhanVien == MaNhanVien);
            if (nv == null)
            {
                return HttpNotFound();
            }

            var model = new NhanVienViewModel
            {
                MaNhanVien = nv.MaNhanVien,
                TenNhanVien = nv.TenNhanVien,
                Email = nv.Email,
                SDT = nv.SDT,
                CCCD = nv.CCCD,
                NamSinh = nv.NamSinh.HasValue ? int.Parse(nv.NamSinh.ToString()) : 0,
                VaiTro = db.TaiKhoanNhanViens.FirstOrDefault(tk => tk.Email == nv.Email)?.VaiTro,
                ChucVu = db.ChucVus.FirstOrDefault(cv => cv.MaChucVu == nv.MaChucVu)?.TenChucVu,
                MoTa = db.ChucVus.FirstOrDefault(cv => cv.MaChucVu == nv.MaChucVu)?.MoTa,
                HeSoLuong = nv.HeSoLuong.HasValue ? (double)nv.HeSoLuong.Value : 0
            };
            return Json(model, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult ThemNhanVien(FormCollection form)
        {
            try
            {
                string tenNhanVien = form["TenNhanVien"];
                string email = form["Email"];
                string sdt = form["SDT"];
                string cccd = form["CCCD"];
                string vaiTro = form["VaiTro"];
                int namSinh = int.Parse(form["NamSinh"]);
                string chucVu = form["ChucVu"];
                string moTa = form["MoTa"];
                decimal luong = decimal.Parse(form["Luong"]);

                string defaultPassword = GenerateRandomPassword(8);

                if (!KiemTraDuLieu.KiemTraSDT_VN(sdt))
                {
                    TempData["Error"] = string.Format("Số điện thoại {0} không đúng định dạng!", sdt);
                    return RedirectToAction("DanhSachNhanVien");

                }

                if (!KiemTraDuLieu.KiemTraEmail(email))
                {
                    TempData["Error"] = string.Format("Email {0} không đúng định dạng!",email);
                    return RedirectToAction("DanhSachNhanVien");
                }

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (var command = new SqlCommand("ThemNhanVien", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("TenNhanVien", tenNhanVien);
                        command.Parameters.AddWithValue("Email", email);
                        command.Parameters.AddWithValue("SDT", sdt);
                        command.Parameters.AddWithValue("CCCD", cccd);
                        command.Parameters.AddWithValue("VaiTro", vaiTro);
                        command.Parameters.AddWithValue("NamSinh", namSinh);
                        command.Parameters.AddWithValue("ChucVu", chucVu);
                        command.Parameters.AddWithValue("MoTa", moTa);
                        command.Parameters.AddWithValue("Luong", luong);
                        command.Parameters.AddWithValue("DefaultPassword", defaultPassword);

                        command.ExecuteNonQuery();
                    }
                }
                TempData["Success"] = "Thêm nhân viên thành công! Mật khẩu đã được gửi về mail của nhân viên.";
                EmailSender emailSender = new EmailSender("smtp.gmail.com", 587, true, "dasnn2004@gmail.com", "ckfq pqmc xkll tgcw");
                emailSender.SendEmail(email, "Mật khẩu nhân viên", getMailBody(tenNhanVien, defaultPassword), true);
                return RedirectToAction("DanhSachNhanVien");
            }
            catch (SqlException ex)
            {
                TempData["Error"] = "Đã xảy ra lỗi khi thực thi câu lệnh SQL: " + ex.Message;
                return RedirectToAction("DanhSachNhanVien");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Đã xảy ra lỗi: " + ex.Message;
                return RedirectToAction("DanhSachNhanVien");
            }

        }


        public ActionResult SuaNhanVien(FormCollection form)
        {
            try
            {
                string maNhanVien = form["MaNhanVien"];
                string tenNhanVien = form["TenNhanVien"];
                string sdt = form["SDT"];
                string cccd = form["CCCD"];
                string vaiTro = form["VaiTro"];
                string chucVu = form["ChucVu"];
                decimal luong = decimal.Parse(form["Luong"]);
                int namSinh = int.Parse(form["NamSinh"]);

                if (!KiemTraDuLieu.KiemTraSDT_VN(sdt))
                {
                    TempData["Error"] = string.Format("Số điện thoại {0} không đúng định dạng!", sdt);
                    return RedirectToAction("DanhSachNhanVien");

                }

                var nhanVien = db.NhanViens.FirstOrDefault(nv => nv.MaNhanVien == maNhanVien);
                if (nhanVien == null)
                {
                    TempData["Error"] = "Nhân viên không tồn tại!";
                    return RedirectToAction("DanhSachNhanVien"); // Hoặc trang phù hợp
                }

                TaiKhoanNhanVien tk = db.TaiKhoanNhanViens.FirstOrDefault(t => t.Email == nhanVien.Email);
                
                tk.VaiTro = vaiTro;
                db.SubmitChanges();
                nhanVien.TenNhanVien = tenNhanVien;
                nhanVien.SDT = sdt;
                nhanVien.CCCD = cccd;
                nhanVien.MaChucVu = db.ChucVus.FirstOrDefault(cv => cv.TenChucVu == chucVu)?.MaChucVu;
                nhanVien.HeSoLuong = luong;
                nhanVien.NamSinh = namSinh;
                db.SubmitChanges();

                TempData["Success"] = "Cập nhật thông tin nhân viên thành công!";
                return RedirectToAction("DanhSachNhanVien");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Đã xảy ra lỗi: " + ex.Message;
                return RedirectToAction("DanhSachNhanVien");
            }
        }

        public ActionResult XoaNhanVien(string id)
        {
            try
            {
                // Tìm nhân viên theo mã nhân viên
                var nhanVien = db.NhanViens.FirstOrDefault(nv => nv.MaNhanVien == id);
                if (nhanVien == null)
                {
                    TempData["Error"] = "Nhân viên không tồn tại!";
                    return RedirectToAction("DanhSachNhanVien"); 
                }
                var tk = db.TaiKhoanNhanViens.FirstOrDefault(t => t.Email == nhanVien.Email);
                if (tk != null)
                {
                    db.TaiKhoanNhanViens.DeleteOnSubmit(tk);
                }
                db.SubmitChanges();

                TempData["Success"] = "Xóa nhân viên thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Đã xảy ra lỗi: " + ex.Message;
            }

            return RedirectToAction("DanhSachNhanVien"); // Quay lại trang danh sách nhân viên
        }


        public static string GenerateRandomPassword(int length)
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            Random random = new Random();
            char[] password = new char[length];

            for (int i = 0; i < length; i++)
            {
                password[i] = validChars[random.Next(validChars.Length)];
            }

            return new string(password);
        }

        public ActionResult KhoiPhucNhanVien(string id)
        {
            try
            {
                var nhanVien = db.NhanViens.FirstOrDefault(nv => nv.MaNhanVien == id);
                if (nhanVien == null)
                {
                    TempData["Error"] = "Nhân viên không tồn tại hoặc chưa bị xóa!";
                    return RedirectToAction("DanhSachNhanVien"); 
                }
                TaiKhoanNhanVien tk = db.TaiKhoanNhanViens.FirstOrDefault(t => t.Email == nhanVien.Email);
                tk.DaXoa = false;

                db.SubmitChanges();

                TempData["Success"] = "Khôi phục nhân viên thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Đã xảy ra lỗi: " + ex.Message;
            }

            return RedirectToAction("DanhSachNhanVien"); // Quay lại danh sách nhân viên
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
            Hệ thống Quản lý Nhân viên
        </div>
        <div class='content'>
            <p>Xin chào <strong>{0}</strong>,</p>
            <p>Chúng tôi vui mừng thông báo rằng tài khoản của bạn đã được tạo thành công trong hệ thống Quản lý Nhân viên.</p>
            <p>Mật khẩu tạm thời của bạn là:</p>
            <p class='highlight'>{1}</p>
            <p>Vui lòng đăng nhập và thay đổi mật khẩu ngay sau khi đăng nhập lần đầu tiên để bảo mật tài khoản của bạn.</p>
            <p>Nếu bạn gặp vấn đề trong quá trình đăng nhập, vui lòng liên hệ với bộ phận hỗ trợ của chúng tôi.</p>
            <p>Trân trọng,</p>
            <p>Đội ngũ hỗ trợ hệ thống</p>
        </div>
        <div class='footer'>
            &copy; 2024 Hệ thống Quản lý Nhân viên. Mọi quyền được bảo lưu.
        </div>
    </div>
</body>
</html>", userName, tempPassword);
        }

    }
}
