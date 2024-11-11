﻿using Newtonsoft.Json;
using QuanLyVeTau.Models;
using QuanLyVeTau.Tools;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

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

        [Authorize]
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




            ViewBag.tongNguoiDung = tongNguoiDung;
            ViewBag.tongVeDaBan = tongVeDaBan;
            ViewBag.tongDoanhThu = tongDoanhThu;
            ViewBag.tongPhanHoi = tongPhanHoi;
            ViewBag.donThuTheoThang = JsonConvert.SerializeObject(doanhThuTheoThang);
            ViewBag.soVeTheoThang = JsonConvert.SerializeObject(soVeTheoThang);
            ViewBag.soKhachHang = soKhachHang;
            ViewBag.soKhuyenMai = soKhuyenMai;

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


        public ActionResult QuanLyKhachHang(string searchKeyword = "", bool? isDeleted = null)
        {
            var query = db.KhachHangs.AsQueryable();

            if (!string.IsNullOrEmpty(searchKeyword))
            {
                query = query.Where(kh => kh.TenKhach.Contains(searchKeyword) || kh.Email.Contains(searchKeyword) || kh.MaKhach.Contains(searchKeyword));
            }

            if (isDeleted.HasValue)
            {
                query = query.Where(kh => db.TaiKhoans.Any(tk => tk.Email == kh.Email && tk.DaXoa == isDeleted));
            }

            var khachHangs = query.ToList();


            return View("QuanLyKhachHang", khachHangs);
        }


    }
}