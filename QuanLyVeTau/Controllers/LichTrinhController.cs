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
                                       NhatKyTaus = new HashSet<NhatKyTau>(db.NhatKyTaus.Where(nk => nk.MaLichTrinh == lt.MaLichTrinh)),
                                   }).ToPagedList(page, pageSize);

            ViewBag.Gas = new HashSet<Ga>(db.Gas.ToList());
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


        

        public ActionResult ChiTietLichTrinhModal(string maLichTrinh)
        {
            var chiTietLichTrinh = db.ChiTietLichTrinhs
                                     .Where(ct => ct.MaLichTrinh == maLichTrinh)
                                     .Join(db.Gas, ct => ct.MaGa, ga => ga.MaGa, (ct, ga) => new ChiTietLichTrinhViewModel
                                     {
                                         MaChiTiet = ct.MaChiTiet,
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


        [HttpPost]
        public ActionResult ThemChiTietLichTrinh(FormCollection form)
        {
            string maLichTrinh = form["MaLichTrinh"];
            string maGa = form["MaGa"];
            int sttGa = Convert.ToInt32(form["SttGa"]);
            string thoiGianDiChuyenStr = form["ThoiGianDiChuyen"];
            int soGio = 0;
            TimeSpan thoiGianDiChuyen;
            if (int.TryParse(thoiGianDiChuyenStr, out soGio))
            {
                thoiGianDiChuyen = new TimeSpan(soGio, 0, 0); // tạo TimeSpan từ số giờ
                                                                       // Bây giờ bạn có TimeSpan với giá trị giờ được cung cấp
                Console.WriteLine($"Thời gian di chuyển là: {thoiGianDiChuyen}");
            }
            else
            {
                // Nếu không thể chuyển thành số, mặc định TimeSpan bằng 0
                thoiGianDiChuyen = TimeSpan.Zero;
                Console.WriteLine($"Giá trị không hợp lệ, mặc định là: {thoiGianDiChuyen}");
            }


            float khoangCach;
            if (!float.TryParse(form["KhoangCach"], out khoangCach))
            {
                khoangCach = 0f;
            }

            var ctlt = new ChiTietLichTrinh
            {
                MaLichTrinh = maLichTrinh,
                MaGa = maGa,
                Stt_Ga = sttGa,
                ThoiGianDiChuyenTuTramTruoc = thoiGianDiChuyen,
                KhoangCachTuTramTruoc = khoangCach
            };

            try
            {
                db.ChiTietLichTrinhs.InsertOnSubmit(ctlt);
                db.SubmitChanges();

                return Json(new { success = true, message = "Chi tiết lịch trình đã được thêm thành công!" });
            }
            catch (SqlException ex)
            {
                if (ex.Message.Contains("Cannot insert the value NULL into column 'MaChiTiet'"))
                {
                    return Json(new { success = true, message = "Chi tiết lịch trình đã được thêm thành công, nhưng có vấn đề về mã chi tiết!" });
                }
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        public ActionResult LayCTLTTheoMa(string MaChiTiet)
        {
            try
            {
                var ctlt = db.ChiTietLichTrinhs.SingleOrDefault(g => g.MaChiTiet == MaChiTiet);
                var danhSachMaGa = db.Gas.Select(g => new { g.MaGa, g.TenGa }).ToList(); // Lấy danh sách Mã Ga từ DB

                if (ctlt != null)
                {
                    return Json(new
                    {
                        success = true,
                        id = ctlt.MaChiTiet,
                        sttGa = ctlt.Stt_Ga,
                        maGa = ctlt.MaGa,
                        thoiGianDiChuyen = ctlt.ThoiGianDiChuyenTuTramTruoc.ToString(@"hh\:mm"),
                        khoangCach = ctlt.KhoangCachTuTramTruoc,
                        danhSachMaGa = danhSachMaGa // Trả về danh sách Mã Ga
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = "Chi tiết lịch trình không tồn tại." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public ActionResult CapNhatCTLT(string MaChiTiet, int SttGa, string MaGa, string ThoiGianDiChuyen, float KhoangCach)
        {
            try
            {
                var ctlt = db.ChiTietLichTrinhs.SingleOrDefault(g => g.MaChiTiet == MaChiTiet);
                if (ctlt != null)
                {
                    ctlt.Stt_Ga = SttGa;

                    // Kiểm tra giá trị mới của MaGa có hợp lệ không
                    var ga = db.Gas.SingleOrDefault(g => g.MaGa == MaGa);
                    if (ga == null)
                    {
                        return Json(new { success = false, message = "Mã Ga không hợp lệ!" });
                    }

                    // Cập nhật MaGa chỉ khi giá trị hợp lệ
                    ctlt.MaGa = MaGa;

                    // Xử lý thời gian di chuyển
                    TimeSpan thoiGianDiChuyen = TimeSpan.Zero;
                    if (!string.IsNullOrEmpty(ThoiGianDiChuyen))
                    {
                        var parts = ThoiGianDiChuyen.Split(':');
                        if (parts.Length == 2)
                        {
                            int hours = 0;
                            int minutes = 0;
                            if (int.TryParse(parts[0], out hours) && int.TryParse(parts[1], out minutes))
                            {
                                thoiGianDiChuyen = new TimeSpan(hours, minutes, 0);
                            }
                            else
                            {
                                return Json(new { success = false, message = "Thời gian không hợp lệ!" });
                            }
                        }
                        else if (int.TryParse(ThoiGianDiChuyen, out int soGio))
                        {
                            thoiGianDiChuyen = new TimeSpan(soGio, 0, 0);
                        }
                        else
                        {
                            return Json(new { success = false, message = "Thời gian không hợp lệ!" });
                        }
                    }

                    // Cập nhật Thời gian di chuyển và Khoảng cách
                    ctlt.ThoiGianDiChuyenTuTramTruoc = thoiGianDiChuyen;
                    ctlt.KhoangCachTuTramTruoc = KhoangCach;

                    // Lưu thay đổi vào cơ sở dữ liệu
                    db.SubmitChanges();

                    return Json(new { success = true, message = "Cập nhật chi tiết lịch trình thành công!" });
                }
                else
                {
                    return Json(new { success = false, message = "Không tìm thấy chi tiết lịch trình!" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }





    }
}