using PagedList;
using QuanLyVeTau.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;

namespace QuanLyVeTau.Controllers
{
    public class KhuyenMaiController : Controller
    {

        private readonly QuanLyVeTauDBDataContext db;

        public KhuyenMaiController()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["QL_VETAUConnectionString4"].ConnectionString;
            db = new QuanLyVeTauDBDataContext(connectionString);
        }


        public ActionResult DanhSachKhuyenMai(string search = "", DateTime? ngayBatDau = null, DateTime? ngayKetThuc = null, int page = 1)
        {
            // Truy vấn khuyến mãi từ database
            var khuyenMais = db.KhuyenMais.AsQueryable();

            // Lọc theo tên khuyến mãi
            if (!string.IsNullOrEmpty(search))
            {
                khuyenMais = khuyenMais.Where(km => km.TenKhuyenMai.ToLower().Contains(search.ToLower()) ||
                                                km.MaKhuyenMai.ToLower().Contains(search.ToLower()));
            }

            // Lọc theo ngày bắt đầu
            if (ngayBatDau.HasValue)
            {
                khuyenMais = khuyenMais.Where(km => km.NgayBatDau >= ngayBatDau.Value);
            }

            // Lọc theo ngày kết thúc
            if (ngayKetThuc.HasValue)
            {
                khuyenMais = khuyenMais.Where(km => km.NgayKetThuc <= ngayKetThuc.Value);
            }

            // Sắp xếp kết quả và phân trang
            int pageSize = 12;
            var result = khuyenMais.OrderBy(km => km.NgayBatDau) // Hoặc theo một trường khác nếu cần
                                   .ToPagedList(page, pageSize);

            // Truyền kết quả tới View
            return View(result);
        }


        public ActionResult TaoKhuyenMai()
        {
            return View();
        }

        [HttpPost]
        public ActionResult TaoKhuyenMai(FormCollection form)
        {
            try
            {
                var khuyenMai = new KhuyenMai
                {
                    TenKhuyenMai = form["TenKhuyenMai"],
                    PhanTramGiam = Double.Parse(form["PhanTramGiam"]),
                    SoTienGiamToiDa = int.Parse(form["SoTienGiamToiDa"]),
                    NgayBatDau = DateTime.Parse(form["NgayBatDau"]),
                    NgayKetThuc = DateTime.Parse(form["NgayKetThuc"]),
                    SoLuong = Convert.ToInt32(form["SoLuong"]),
                    SoLuongConLai = Convert.ToInt32(form["SoLuong"]), 
                };

                db.KhuyenMais.InsertOnSubmit(khuyenMai);
                db.SubmitChanges();

                return RedirectToAction("DanhSachKhuyenMai");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi thêm khuyến mãi: " + ex.Message);
            }

            return View();
        }
    }
}