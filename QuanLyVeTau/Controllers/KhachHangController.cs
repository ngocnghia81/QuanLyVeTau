using PagedList;
using QuanLyVeTau.Models;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace QuanLyVeTau.Controllers
{
    public class KhachHangController : Controller
    {
        private readonly QuanLyVeTauDBDataContext db;

        public KhachHangController()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["QL_VETAUConnectionString"].ConnectionString;
            db = new QuanLyVeTauDBDataContext(connectionString);
        }

        public ActionResult DanhSachKhachHang(string searchKeyword = "", bool? isDeleted = null, int page = 1)
        {
            var query = db.KhachHangs.Include(kh => kh.TaiKhoans).AsQueryable();

            if (!string.IsNullOrEmpty(searchKeyword))
            {
                query = query.Where(kh => kh.TenKhach.Contains(searchKeyword) ||
                                          kh.Email.Contains(searchKeyword) ||
                                          kh.SDT.Contains(searchKeyword) ||
                                          kh.CCCD.Contains(searchKeyword) ||
                                          kh.MaKhach.Contains(searchKeyword));
            }

            if (isDeleted.HasValue)
            {
                query = query.Where(kh => kh.TaiKhoans != null && kh.TaiKhoans[0].DaXoa == isDeleted);
            }

            var khachHangs = query.ToList();

            ViewBag.ActiveLink = "manageCustomersLink";

            // Áp dụng phân trang
            int pageSize = 12;  // Số vé hiển thị trên mỗi trang
            var result = khachHangs.ToPagedList(page, pageSize);  // Phân trang danh sách vé

            // Trả về view với danh sách vé đã phân trang
            return View(result); // Đây sẽ trả về IPagedList<Ve> cho View

        }


        public ActionResult KhoaTaiKhoan(FormCollection form)
        {
            string id = form["id"];

            if (!string.IsNullOrEmpty(id))
            {
                var khachHang = db.KhachHangs.FirstOrDefault(kh => kh.MaKhach == id);

                if (khachHang != null && khachHang.TaiKhoans.Count > 0)
                {
                    khachHang.TaiKhoans[0].DaXoa = true;
                    db.SubmitChanges();
                    return RedirectToAction("DanhSachKhachHang");
                }
                else
                {
                    ViewBag.ErrorMessage = "Khách hàng không tồn tại hoặc tài khoản không hợp lệ.";
                }
            }
            else
            {
                ViewBag.ErrorMessage = "ID không hợp lệ.";
            }

            return View();
        }

        public ActionResult MoTaiKhoan(FormCollection form)
        {
            string id = form["id"];

            if (!string.IsNullOrEmpty(id))
            {
                var khachHang = db.KhachHangs.FirstOrDefault(kh => kh.MaKhach == id);

                if (khachHang != null && khachHang.TaiKhoans.Count > 0)
                {
                    khachHang.TaiKhoans[0].DaXoa = false;
                    db.SubmitChanges();
                    return RedirectToAction("DanhSachKhachHang");
                }
                else
                {
                    ViewBag.ErrorMessage = "Khách hàng không tồn tại hoặc tài khoản không hợp lệ.";
                }
            }
            else
            {
                ViewBag.ErrorMessage = "ID không hợp lệ.";
            }
            return View();
        }


        public ActionResult XemLichSuGiaoDich(string id)
        {
            var khachHang = db.KhachHangs.FirstOrDefault(kh => kh.MaKhach == id);
            if (khachHang == null)
            {
                return HttpNotFound("Không tìm thấy khách hàng.");
            }

            var lichSuGiaoDich = db.HoaDons
                .Where(hd => hd.MaKhach == id)
                .Select(hd => new HoaDonViewModel
                {
                    HoaDon = hd,
                    Ve = db.Ves.FirstOrDefault(v => v.MaHoaDon == hd.MaHoaDon),
                    KhuyenMai = db.KhuyenMais.FirstOrDefault(km => km.MaKhuyenMai == hd.MaKhuyenMai)
                })
                .ToList();

            ViewBag.KhachHangId = id;
            ViewBag.TenKhachHang = khachHang.TenKhach;
            return View(lichSuGiaoDich);
        }

        public ActionResult XemPhanHoi(string id = null)
        {
            List<PhanHoi> phanHois = db.PhanHois.Where(p => p.HoaDon.MaKhach == id).ToList();
            return View(phanHois);
        }
    }
}