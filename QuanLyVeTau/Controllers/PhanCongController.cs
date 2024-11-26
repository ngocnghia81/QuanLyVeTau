using Microsoft.Ajax.Utilities;
using QuanLyVeTau.Models;
using QuanLyVeTau.Tools;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
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
                    NhanViens = db.PhanCongs
                            .Where(pc => pc.MaNhatKy == data.MaNhatKy)  // Lọc theo MaNhatKy
                            .Join(db.NhanViens,
                                  pc => pc.MaNhanVien,   // Join vào bảng NhanViens theo MaNhanVien
                                  nv => nv.MaNhanVien,   // Điều kiện join trên MaNhanVien
                                  (pc, nv) => nv)
                            .Distinct()
                            .ToList(),
                    ChucVus = db.PhanCongs
                        .Where(pc => pc.MaNhatKy == data.MaNhatKy)
                        .Join(db.NhanViens, pc => pc.MaNhanVien, nv => nv.MaNhanVien, (pc, nv) => nv.MaChucVu)
                        .Join(db.ChucVus, maChucVu => maChucVu, cv => cv.MaChucVu, (maChucVu, cv) => cv.TenChucVu)
                        .Distinct()
                        .ToList(),
                    ChuaPhanCong = string.IsNullOrEmpty(data.TenNhanVien)
                })
                .DistinctBy(pc => pc.MaNhatKy)
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


        [HttpGet]
        public JsonResult LayNhanVienPhuHopNhatKy(string maNhatKy)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                string sql = "EXEC LayNhanVienChuaPhanCong @MaNhatKyChon";

                using (var command = new SqlCommand(sql, connection))
                {
                    // Thêm tham số vào stored procedure
                    command.Parameters.AddWithValue("@MaNhatKyChon", maNhatKy);

                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        var nhanVienList = new List<dynamic>();

                        while (reader.Read())
                        {
                            nhanVienList.Add(new
                            {
                                MaNhanVien = reader["MaNhanVien"],
                                TenNhanVien = reader["TenNhanVien"],
                                MaChucVu = reader["MaChucVu"],
                                Email = reader["Email"],
                                SDT = reader["SDT"],
                                CCCD = reader["CCCD"],
                                NamSinh = reader["NamSinh"],
                                HeSoLuong = reader["HeSoLuong"],
                                ChucVu = db.ChucVus
                                    .Join(db.NhanViens,
                                          cv => cv.MaChucVu,
                                          nv => nv.MaChucVu,
                                          (cv, nv) => new { cv.TenChucVu, nv.MaNhanVien })
                                    .Where(x => x.MaNhanVien == (string)reader["MaNhanVien"])
                                    .Select(x => x.TenChucVu)
                                    .FirstOrDefault()

                            });
                        }

                        nhanVienList = nhanVienList
                            .OrderBy(nv => nv.TenNhanVien.Split(' ')[nv.TenNhanVien.Split(' ').Length - 1])
                            .ToList();

                        return Json(nhanVienList, JsonRequestBehavior.AllowGet);
                    }
                }
            }
        }


        [HttpPost]
        public JsonResult XoaNhanVien(string maNhanVien)
        {
            try
            {
                var assignment = db.PhanCongs.FirstOrDefault(pc => pc.MaNhanVien == maNhanVien);
                if (assignment != null)
                {
                    db.PhanCongs.DeleteOnSubmit(assignment);
                    db.SubmitChanges();
                    return Json(new { success = true });
                }

                return Json(new { success = false, errorMessage = "Nhân viên không tồn tại trong cơ sở dữ liệu" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMessage = ex.Message });
            }
        }
    }
}
