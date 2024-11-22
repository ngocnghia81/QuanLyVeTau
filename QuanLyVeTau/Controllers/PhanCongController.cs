using QuanLyVeTau.Models;
using QuanLyVeTau.Tools;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace QuanLyVeTau.Controllers
{
    public class PhanCongController : Controller
    {
        private readonly QuanLyVeTauDBDataContext db;
        private readonly string connectionString;

        public PhanCongController()
        {
            connectionString = ConfigurationManager.ConnectionStrings["QL_VETAUConnectionString1"].ConnectionString;
            db = new QuanLyVeTauDBDataContext(connectionString);
        }

        public List<NhanVien> LayDanhSachNhanVien()
        {
            return db.NhanViens.ToList();
        }

        public ActionResult XemLichPhanCong(DateTime? ngayBatDau = null)
        {
            if (ngayBatDau == null)
            {
                ngayBatDau = DateTime.Now;
            }
            // Tính ngày đầu và ngày cuối tuần
            DateTime startOfWeek = ngayBatDau.Value;
            DateTime endOfWeek = startOfWeek.AddDays(6);

            // Truy vấn lịch phân công từ cơ sở dữ liệu
            var lichPhanCong = db.Vw_LichPhanCongs
                .Where(data => data.NgayGio >= startOfWeek && data.NgayGio <= endOfWeek)
                .Select(data => new PhanCongViewModel
                {
                    MaPhanCong = data.MaPhanCong,
                    MaNhatKy = data.MaNhatKy,
                    MaLichTrinh = data.MaLichTrinh,
                    NgayGio = data.NgayGio,
                    TrangThai = data.TrangThai,
                    SDT = data.SDT,
                    NhanViens = db.PhanCongs
                        .Where(pc => pc.MaNhatKy == data.MaNhatKy)
                        .Join(db.NhanViens, pc => pc.MaNhanVien, nv => nv.MaNhanVien, (pc, nv) => nv)
                        .ToList(),
                    ChucVus = db.PhanCongs
                        .Where(pc => pc.MaNhatKy == data.MaNhatKy)
                        .Join(db.NhanViens, pc => pc.MaNhanVien, nv => nv.MaNhanVien, (pc, nv) => nv.MaChucVu)
                        .Join(db.ChucVus, maChucVu => maChucVu, cv => cv.MaChucVu, (maChucVu, cv) => cv.TenChucVu)
                        .ToList(),
                    ChuaPhanCong = string.IsNullOrEmpty(data.TenNhanVien)
                })
                .ToList();

            ViewBag.Employees = db.Vw_NhanVienDangHoatDongs.ToList();
            ViewBag.Logbooks = db.Vw_NhatKyTauChuaHoanThanhs.OrderByDescending(nk => nk.NgayGio).ToList();

            ViewBag.ActiveLink = "manageAssignments";
            ViewBag.StartOfWeek = startOfWeek;

            return View(lichPhanCong);
        }

        public string PhanCong(string maNhanVien, string maPhanCong, string maNhatKy)
        {
            try
            {
                var employee = db.NhanViens.SingleOrDefault(e => e.MaNhanVien == maNhanVien);
                var logbook = db.NhatKyTaus.SingleOrDefault(l => l.MaNhatKy == maNhatKy);

                if (employee == null || logbook == null)
                {
                    return "Không tìm thấy nhân viên hoặc nhật ký."; 
                }

                var assignment = new PhanCong
                {
                    MaNhanVien = maNhanVien,
                    MaNhatKy = maNhatKy
                };

                db.PhanCongs.InsertOnSubmit(assignment);
                db.SubmitChanges(); 

                return "Phân công thành công.";
            }
            catch (Exception ex)
            {
                return "Lỗi: "+ ex.Message; 
            }
        }


        [HttpPost]
        public ActionResult PhanCongNhanVien(string maNhanVien, string maPhanCong, string maNhatKy)
        {
            try
            {
                var employee = db.NhanViens.SingleOrDefault(e => e.MaNhanVien == maNhanVien);
                var logbook = db.NhatKyTaus.SingleOrDefault(l => l.MaNhatKy == maNhatKy);

                if (employee == null || logbook == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy nhân viên hoặc nhật ký.";
                    return RedirectToAction("XemLichPhanCong");
                }

                var assignment = new PhanCong
                {
                    MaPhanCong = maPhanCong,
                    MaNhanVien = maNhanVien,
                    MaNhatKy = maNhatKy
                };

                db.PhanCongs.InsertOnSubmit(assignment);
                db.SubmitChanges();

                TempData["SuccessMessage"] = "Phân công thành công.";
                return RedirectToAction("XemLichPhanCong");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
                return RedirectToAction("XemLichPhanCong");
            }
        }
    }
}
