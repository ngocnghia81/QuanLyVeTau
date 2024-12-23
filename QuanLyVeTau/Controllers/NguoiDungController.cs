﻿using QuanLyVeTau.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;

namespace QuanLyVeTau.Controllers
{
    public class NguoiDungController : Controller
    {
        
        string connectionString = ConfigurationManager.ConnectionStrings["QL_VETAUConnectionString"].ConnectionString;
        QuanLyVeTauDBDataContext db;
        public NguoiDungController()
        {
            db =new QuanLyVeTauDBDataContext(connectionString);
        }
        // GET: NguoiDung

        public ActionResult Index()
        {
            return View();
        }

        [CustomRoleAuthorizeAttribute("User")]
        public ActionResult HoaDon(string mahoadon)
        {

            HoaDon hoaDon = db.HoaDons.FirstOrDefault(t => t.MaHoaDon == mahoadon);
            List<Ve> DanhSachVe = db.Ves.Where(t => t.MaHoaDon == mahoadon).ToList();
            PhanHoi phanHoi = db.PhanHois.FirstOrDefault(t => t.MaHoaDon == mahoadon);
            

            ViewBag.HoaDon = hoaDon;
            ViewBag.DanhSachVe = DanhSachVe;
            double TienKhuyenMai = 0;
            double TongTien = 0;
            try
            {
                KhuyenMai khuyenMai = db.KhuyenMais.First(t => t.MaKhuyenMai == hoaDon.MaKhuyenMai);
                double thanhTien = (double)hoaDon.ThanhTien;
                double phanTram = khuyenMai.PhanTramGiam.Value / 100 * 1.0;
                TongTien = thanhTien  / (1 - phanTram );
                TienKhuyenMai = TongTien * phanTram;
                TienKhuyenMai = Math.Min(TienKhuyenMai, khuyenMai.SoTienGiamToiDa.Value);
                TongTien = TienKhuyenMai + thanhTien;
            }
            catch
            {
                TongTien = (double)hoaDon.ThanhTien;
                TienKhuyenMai = 0;
            }

            KhachHang khachHang = db.KhachHangs.FirstOrDefault(t => t.MaKhach == hoaDon.MaKhach);

            List<string> dsDi = new List<string>();
            List<string> dsDen = new List<string>();
            List<string> dsTau = new List<string>();
            List<string> dsGhe = new List<string>();
            List<string> dsThoiGian = new List<string>();
            List<bool> isDone = new List<bool>();
            foreach (Ve ve in DanhSachVe)
            {
               
                string ghe = "";
                string Thoigian = "";
                ChiTietLichTrinh Di = db.ChiTietLichTrinhs.First(t => t.MaChiTiet == ve.DiemDi);
                Ga gadi = db.Gas.First(t => t.MaGa == Di.MaGa);

                ChiTietLichTrinh Den = db.ChiTietLichTrinhs.First(t => t.MaChiTiet == ve.DiemDen);
                Ga gaden = db.Gas.First(t => t.MaGa == Den.MaGa);

                Khoang khoang = db.Khoangs.First(t => t.MaKhoang == ve.MaKhoang);
                Toa toa = db.Toas.First(t => t.MaToa == khoang.MaToa);
                NhatKyTau nk = db.NhatKyTaus.First(t => t.MaNhatKy == ve.MaNhatKy);
                Tau tau = db.Taus.First(t => t.MaTau == toa.MaTau);
                List<ChiTietLichTrinh> chiTiets = db.ChiTietLichTrinhs.Where(t => t.MaLichTrinh == nk.MaLichTrinh).ToList();
                DateTime thoigiandichuyen = nk.NgayGio;
                foreach (ChiTietLichTrinh ctlt in chiTiets.OrderBy(t=>t.Stt_Ga).ToList() )
                {
                    
                    thoigiandichuyen = thoigiandichuyen.Add(ctlt.ThoiGianDiChuyenTuTramTruoc);
                    if (ctlt.Stt_Ga == Di.Stt_Ga)
                        Thoigian += thoigiandichuyen.ToString("dd-MM HH'g'mm'p'") + " đến ";

                    if (ctlt.Stt_Ga == 1)
                        continue;

                    if (ctlt.Stt_Ga == Den.Stt_Ga)
                    {
                        Thoigian += thoigiandichuyen.ToString("dd-MM HH'g'mm'p'");
                        break;
                    }

                    thoigiandichuyen = thoigiandichuyen.AddMinutes(15);
                }


                ghe = string.Format("T-{0} K-{1} G-{2}", toa.SoToa, khoang.SoKhoang, ve.Stt_Ghe);

                dsThoiGian.Add(Thoigian);
                dsTau.Add(tau.TenTau.ToString());
                dsDi.Add(gadi.TenGa.ToString());
                dsDen.Add(gaden.TenGa.ToString());
                dsGhe.Add(ghe);
                isDone.Add(ve.NhatKyTau.NgayGio < DateTime.Now);
            }
            ViewBag.isDone = isDone;
            ViewBag.dsDi = dsDi;
            ViewBag.dsDen = dsDen;
            ViewBag.dsGhe = dsGhe;
            ViewBag.dsTau = dsTau;
            ViewBag.dsThoiGian = dsThoiGian;
            ViewBag.TenKhachHang = khachHang.TenKhach;
            ViewBag.TongTien = TongTien;
            ViewBag.TienKhuyenMai = TienKhuyenMai;
            ViewBag.PhanHoi = phanHoi;
            return View();
        }

        [CustomRoleAuthorizeAttribute("User")]
        public ActionResult Lichsu()
        {
            KhachHang khachHang = db.KhachHangs.First(t=>t.Email == User.Identity.Name);
            List<HoaDon> hoaDons = db.HoaDons.Where(t => t.MaKhach == khachHang.MaKhach).ToList();
            hoaDons = hoaDons.OrderByDescending(t => t.ThoiGianLapHoaDon).ToList();
            List<double> TienGiams = new List<double>();
            foreach (HoaDon hoaDon in hoaDons)
            {
                KhuyenMai khuyenMai = db.KhuyenMais.FirstOrDefault(t => t.MaKhuyenMai == hoaDon.MaKhuyenMai);

                if (khuyenMai != null)
                {
                    
                    double thanhTien = (double)hoaDon.ThanhTien;
                    double phanTram = khuyenMai.PhanTramGiam.Value / 100 * 1.0;
                    double TienKhuyenMai = thanhTien *phanTram / (1 - phanTram);
                    TienKhuyenMai = Math.Min(TienKhuyenMai, khuyenMai.SoTienGiamToiDa.Value);
                    TienGiams.Add(TienKhuyenMai);

                }
                else
                {
                    TienGiams.Add(0);
                }
                
                
            }
            ViewBag.TienGiams = TienGiams;
            return View(hoaDons);
        }

        [HttpPost]
        [CustomRoleAuthorizeAttribute("User")]
        public JsonResult TraVe(string maVe)
        {
            try 
            {           
                bool success = TraVeDatabase(maVe); 

                if (success)
                {
                    return Json(new { success = true, message = "Trả vé thành công." });
                }
                else
                {
                    return Json(new { success = false, message = "Trả vé thất bại." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }
        private bool TraVeDatabase(string maVe)
        {
            try
            {
                string query = string.Format("exec dbo.TraVe '{0}'", maVe);
                db.ExecuteCommand(query);
                return true;
            }
            catch
            {
                return false;
            }
        }

        [CustomRoleAuthorizeAttribute("User")]
        public ActionResult CapNhatThongTin(string email,string message = null)
        {
            string sql = string.Format("select * from KhachHang where email = '{0}'", email);
            KhachHang khachHang = db.KhachHangs.FirstOrDefault(h => h.Email == email);
            ViewBag.khach = khachHang;
            ViewBag.ErrorMessage = message;
            return View(khachHang);
        }
        [HttpPost]
        [CustomRoleAuthorizeAttribute("User")]
        public ActionResult CapNhat(FormCollection form)
        {
            
            string hoTen = form["HoTen"];
            DateTime namSinh = DateTime.Parse(form["NamSinh"]);
            string sdt = form["SDT"];
            string cccd = form["CCCD"];
            string diaChi = form["DiaChi"];
            string email = form["Email"];

            string ErrorMessage = null;

            KhachHang khachHang = db.KhachHangs.First(t => t.Email == email);

            
            try
            {
                khachHang.TenKhach = hoTen;
                khachHang.NamSinh = namSinh;
                khachHang.SDT = sdt;
                khachHang.CCCD = cccd;
                khachHang.DiaChi = diaChi;

                db.SubmitChanges();

            }
            catch (Exception ex)
            {
                if (db.KhachHangs.FirstOrDefault(t=>t.CCCD == cccd && t.Email != email) != null)
                    ErrorMessage = "CCCD đã được sử dụng";
                else if (db.KhachHangs.FirstOrDefault(t => t.SDT == sdt && t.Email != email) != null)
                    ErrorMessage = "SDT đã được sử dụng";
                else
                {
                    string error = ex.Message.Split('.')[0];
                    ErrorMessage =  error;

                }
                
 
                
            }
            return RedirectToAction("CapNhatThongTin", new { email = form["email"],message = ErrorMessage });
        }
        
    }
}