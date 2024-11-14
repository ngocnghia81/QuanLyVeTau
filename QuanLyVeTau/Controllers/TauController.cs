using PagedList;
using QuanLyVeTau.Models;
using System;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;

namespace QuanLyVeTau.Controllers
{
    public class TauController : Controller
    {

        private readonly QuanLyVeTauDBDataContext db;

        public TauController()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["QL_VETAUConnectionString1"].ConnectionString;
            db = new QuanLyVeTauDBDataContext(connectionString);
        }

        public ActionResult DanhSachTau(string search = "", bool? daXoa = null, DateTime? ngayChay = null, int page = 1)
        {
            var taus = db.Taus.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                taus = taus.Where(t => t.TenTau.ToLower().Contains(search.ToLower()) ||
                    t.MaTau.ToLower().Contains(search));
            }

            if (daXoa.HasValue)
            {
                taus = taus.Where(t => t.DaXoa == daXoa.Value);
            }

            if (ngayChay.HasValue)
            {
                taus = taus.Where(t => db.NhatKyTaus.Any(nk => nk.MaTau == t.MaTau && nk.NgayGio.Date == ngayChay.Value.Date));
            }

            int pageSize = 10;
            var result = taus.OrderBy(t => t.MaTau).ToPagedList(page, pageSize);

            ViewBag.daXoa = daXoa ?? false;
            ViewBag.search = search;
            ViewBag.NgayChay = ngayChay;

            return View(result);
        }


        [HttpPost]
        public ActionResult SuaTau(FormCollection form)
        {
            string maTau = form["MaTau"];
            string tenTau = form["TenTau"];

            if (string.IsNullOrEmpty(maTau) || string.IsNullOrEmpty(tenTau))
            {
                TempData["ErrorMessage"] = "Thông tin tàu không hợp lệ!";
                return RedirectToAction("DanhSachTau");
            }

            var tau = db.Taus.FirstOrDefault(t => t.MaTau == maTau);
            if (tau != null)
            {
                tau.TenTau = tenTau;

                db.SubmitChanges();

                TempData["SuccessMessage"] = "Cập nhật tàu thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy tàu!";
            }
            return RedirectToAction("DanhSachTau");
        }

        [HttpPost]
        public ActionResult ThemTau(FormCollection form)
        {
            string tenTau = form["TenTau"];
            if (string.IsNullOrEmpty(tenTau))
            {
                ModelState.AddModelError("TenTau", "Tên tàu không được để trống.");
                return View();  
            }

            Tau newTau = new Tau
            {
                TenTau = tenTau,
            };

            db.Taus.InsertOnSubmit(newTau);
            db.SubmitChanges();

            return RedirectToAction("DanhSachTau");
        }



    }
}