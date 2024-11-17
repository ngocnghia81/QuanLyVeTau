using PagedList;
using QuanLyVeTau.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace QuanLyVeTau.Controllers
{
    public class LichTrinhController : Controller
    {
        private readonly QuanLyVeTauDBDataContext db;

        public LichTrinhController()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["QL_VETAUConnectionString1"].ConnectionString;
            db = new QuanLyVeTauDBDataContext(connectionString);
        }


        public ActionResult DanhSachLichTrinh(string search = "", DateTime? ngayGio = null, string trangThai = "", int page = 1)
        {
            var lichTrinhs = db.LichTrinhTaus.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                lichTrinhs = lichTrinhs.Where(lt => lt.TenLichTrinh.ToLower().Contains(search) || lt.MaLichTrinh.ToLower().Contains(search));
            }

            if (ngayGio.HasValue)
            {
                var nhatKys = db.NhatKyTaus.Where(nk => nk.NgayGio.Date == ngayGio.Value.Date).Select(nk => nk.MaLichTrinh).Distinct();
                lichTrinhs = lichTrinhs.Where(lt => nhatKys.Contains(lt.MaLichTrinh));
            }

            if (!string.IsNullOrEmpty(trangThai))
            {
                trangThai = trangThai.ToLower();
                lichTrinhs = lichTrinhs.Where(lt => lt.TrangThai.ToLower() == trangThai);
            }

            int pageSize = 12;
            var result = lichTrinhs.OrderByDescending(lt => lt.MaLichTrinh)
                                   .Select(lt => new LichTrinhViewModel
                                   {
                                       MaLichTrinh = lt.MaLichTrinh,
                                       TenLichTrinh = lt.TenLichTrinh,
                                       TrangThai = lt.TrangThai,
                                       NhatKyTaus = new HashSet<NhatKyTau>(db.NhatKyTaus.Where(nk => nk.MaLichTrinh == lt.MaLichTrinh))
                                   }).ToPagedList(page, pageSize);

            
            return View(result);
        }

        [HttpPost]
        public ActionResult ThemLichTrinh(FormCollection form)
        {
            string tienTo = form["tuyen"];
            string tenLichTrinh = form["tenLichTrinh"];
            if (string.IsNullOrWhiteSpace(tienTo) || string.IsNullOrWhiteSpace(tenLichTrinh))
            {
                TempData["ErrorMessage"] = "Tiền tố và tên lịch trình không được để trống!";
                return RedirectToAction("DanhSachLichTrinh");
            }

            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["QL_VETAUConnectionString3"].ConnectionString;

                string sql = string.Format("EXEC ThemLichTrinh '{0}', N'{1}'", tienTo, tenLichTrinh.Trim());

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }

                TempData["SuccessMessage"] = "Lịch trình đã được thêm thành công!";
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi thêm lịch trình: " + ex.Message);

                TempData["ErrorMessage"] = "Có lỗi xảy ra trong quá trình thêm lịch trình. Vui lòng thử lại!";
            }
            return RedirectToAction("DanhSachLichTrinh");
        }


        public class ChiTietLichTrinhViewModel
        {
            public int Stt_Ga { get; set; }
            public string GaTen { get; set; }
            public string GaDiaChi { get; set; }
            public TimeSpan ThoiGianDiChuyenTuTramTruoc { get; set; }
            public double KhoangCachTuTramTruoc { get; set; }
        }

        public ActionResult ChiTietLichTrinhModal(string maLichTrinh)
        {
            var chiTietLichTrinh = db.ChiTietLichTrinhs
                                     .Where(ct => ct.MaLichTrinh == maLichTrinh)
                                     .Join(db.Gas, ct => ct.MaGa, ga => ga.MaGa, (ct, ga) => new ChiTietLichTrinhViewModel
                                     {
                                         Stt_Ga = ct.Stt_Ga,
                                         GaTen = ga.TenGa,
                                         GaDiaChi = ga.DiaChi,
                                         ThoiGianDiChuyenTuTramTruoc = ct.ThoiGianDiChuyenTuTramTruoc,
                                         KhoangCachTuTramTruoc = ct.KhoangCachTuTramTruoc
                                     })
                                     .OrderBy(ct => ct.Stt_Ga)
                                     .ToList();

            return PartialView("_ChiTietLichTrinhModal", chiTietLichTrinh);
        }

        [HttpPost]
        public ActionResult CapNhatTrangThai(string id, string trangThai)
        {
            try
            {
                var lichTrinh = db.LichTrinhTaus.FirstOrDefault(lt => lt.MaLichTrinh == id);

                if (lichTrinh == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy lịch trình!" });
                }

                var validStatuses = new[] { "Đang hoạt động", "Tạm ngưng", "Ngưng" };
                if (!validStatuses.Contains(trangThai))
                {
                    return Json(new { success = false, message = "Trạng thái không hợp lệ!" });
                }

                lichTrinh.TrangThai = trangThai;
                db.SubmitChanges();

                return Json(new { success = true, message = "Cập nhật trạng thái thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }



        [HttpPost]
        public ActionResult DoiTenLichTrinh(FormCollection collection)
        {
            string lichTrinhId = collection["lichTrinhId"]; 
            string tenLichTrinhMoi = collection["tenLichTrinhMoi"];

            try
            {
                var lichTrinh = db.LichTrinhTaus.FirstOrDefault(lt => lt.MaLichTrinh == lichTrinhId);

                if (lichTrinh == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy lịch trình!";
                    return RedirectToAction("DanhSachLichTrinh");
                }

                lichTrinh.TenLichTrinh = tenLichTrinhMoi;
                db.SubmitChanges();

                TempData["SuccessMessage"] = "Cập nhật tên lịch trình thành công!";
                return RedirectToAction("DanhSachLichTrinh");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("DanhSachLichTrinh");
            }
        }

    }
}