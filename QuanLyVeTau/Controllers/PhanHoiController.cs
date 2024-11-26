using Antlr.Runtime.Tree;
using PagedList;
using QuanLyVeTau.Models;
<<<<<<< HEAD
using System.Configuration;
=======
using System;
using System.Configuration;
using System.Diagnostics;
>>>>>>> PhanHoi
using System.Linq;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace QuanLyVeTau.Controllers
{
    public class PhanHoiController : Controller
    {
        private readonly QuanLyVeTauDBDataContext db;
        private readonly string connectionString;

        public PhanHoiController()
        {
<<<<<<< HEAD
            connectionString = ConfigurationManager.ConnectionStrings["QL_VETAUConnectionString4"].ConnectionString;
            db = new QuanLyVeTauDBDataContext(connectionString);
        }

<<<<<<< HEAD
        public ActionResult PhanHoi()
=======
            connectionString = ConfigurationManager.ConnectionStrings["QL_VETAUConnectionString3"].ConnectionString;
            db = new QuanLyVeTauDBDataContext(connectionString);
        }

        [HttpPost]
        public ActionResult PhanHoi(FormCollection form, string mahoadon)
>>>>>>> PhanHoi
        {
            string noiDung = form["NoiDung"];
            int soSao = int.Parse(form["SoSao"]);

            string sql = string.Format("INSERT INTO PhanHoi (MaHoaDon, NoiDung, NgayPhanHoi, SoSao) " +
                                       "VALUES ('{0}', N'{1}', Getdate(), {2})",
                                       mahoadon, noiDung, soSao);
            Debug.WriteLine(sql);
            db.ExecuteCommand(sql);
            return RedirectToAction("HoaDon", "NguoiDung", new { mahoadon });
        }
<<<<<<< HEAD
=======
        public ActionResult DanhSachPhanHoi(string search = "", int? soSao = null, string trangThai = "",int page = 1)
=======

        public ActionResult DanhSachPhanHoi(string search = "", int? soSao = null, string trangThai = "", int page = 1)
>>>>>>> PhanHoi
        {
            var phanHois = db.PhanHois.AsQueryable();

            if (!string.IsNullOrEmpty(trangThai))
            {
                phanHois = phanHois.Where(p => p.TrangThai == trangThai);
            }

            if (!string.IsNullOrEmpty(search))
            {
                phanHois = phanHois
                    .Where(p => p.HoaDon.MaKhach.ToLower().Contains(search.ToLower()) ||
                            p.HoaDon.KhachHang.TenKhach.ToLower().Contains(search.ToLower()) ||
                            p.HoaDon.KhachHang.Email.ToLower().Contains(search.ToLower()) ||
                            p.HoaDon.Ves.Any(v => v.Khoang.Toa.MaTau.ToLower().Contains(search.ToLower())));
            }
            if (soSao.HasValue)
            {
                phanHois = phanHois.Where(p => p.SoSao == soSao.Value);
            }
            int pageSize = 12;
            var pagedResult = phanHois.ToPagedList(page, pageSize);

            var sortedResult = pagedResult.OrderByDescending(p => p.NgayPhanHoi).ToList();

            var pagedSortedResult = sortedResult.ToPagedList(page, pageSize);

            return View(pagedSortedResult);
        }

<<<<<<< HEAD
        [HttpPost]
        public JsonResult CapNhatTrangThai(string id, string trangThai)
        {
            var phanHoi = db.PhanHois.SingleOrDefault(p => p.MaPhanHoi == id);

            if (phanHoi != null)
            {
                if (phanHoi.TrangThai != "Đã xử lý")
                {
                    phanHoi.TrangThai = trangThai;
                    db.SubmitChanges();
                    return Json(new { success = true, newTrangThai = phanHoi.TrangThai });
                }
                else
                {
                    return Json(new { success = false, message = "Không thể thay đổi trạng thái khi phản hồi đã xử lý." });
                }
            }

            return Json(new { success = false, message = "Phản hồi không tồn tại." });
        }

>>>>>>> c670abc483c145cad0e84c39937f9dbd55c06186
    }
=======
       
        //public JsonResult CapNhatTrangThai(string id, string trangThai)
        //{
        //    var phanHoi = db.PhanHois.SingleOrDefault(p => p.MaPhanHoi == id);

        //    if (phanHoi != null)
        //    {
        //        if (phanHoi.TrangThai != "Đã xử lý")
        //        {
        //            phanHoi.TrangThai = trangThai;
        //            db.SubmitChanges();
        //            return Json(new { success = true, newTrangThai = phanHoi.TrangThai });
        //        }
        //        else
        //        {
        //            return Json(new { success = false, message = "Không thể thay đổi trạng thái khi phản hồi đã xử lý." });
        //        }
        //    }

        //    return Json(new { success = false, message = "Phản hồi không tồn tại." });
        //}

     }
>>>>>>> PhanHoi
}
