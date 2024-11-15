using PagedList;
using QuanLyVeTau.Models;
using System;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
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



        public ActionResult XoaTau(string maTau)
        {
            var tau = db.Taus.FirstOrDefault(t => t.MaTau == maTau);

            if (tau != null)
            {
                try
                {
                    db.Taus.DeleteOnSubmit(tau);
                    db.SubmitChanges();
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = ex.Message;

                }
            }
            else
            {
                TempData["ErrorMessage"] = "Tàu không tồn tại.";
            }

            return RedirectToAction("DanhSachTau");
        }

        public ActionResult KhoiPhucTau(string maTau)
        {
            var tau = db.Taus.FirstOrDefault(t => t.MaTau == maTau);

            if (tau != null)
            {
                tau.DaXoa = false;
                db.SubmitChanges();
            }
            else
            {
                TempData["ErrorMessage"] = "Tàu không tồn tại.";
            }

            return RedirectToAction("DanhSachTau");
        }

        [HttpPost]
        public ActionResult TaoToa(FormCollection form)
        {
            int soToa = Convert.ToInt32(form["SoToa"]);
            string maTau = form["maTau"];
            string maLoai = form["LoaiToa"];
            var newToa = new Toa
            {
                SoToa = soToa,
                MaTau = maTau,
                MaLoaiToa = maLoai
            };

            try
            {
                db.Toas.InsertOnSubmit(newToa);
                db.SubmitChanges();
                TempData["SuccessMessage"] = "Tạo toa mới thành công.";
                return RedirectToAction("XemToa", new { maTau = maTau });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("XemToa", new { maTau = maTau });
            }            
        }

        public ActionResult XoaToa(string maToa)
        {
            var toa = db.Toas.FirstOrDefault(t => t.MaToa == maToa);

            try
            {
                if (toa != null)
                {
                    db.Toas.DeleteOnSubmit(toa);
                    db.SubmitChanges();
                }
                return RedirectToAction("XemToa", new { maTau = toa.MaTau });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("XemToa", new { maTau = toa.MaTau });
            }
        }

        public ActionResult CapNhatToa(string maToa)
        {
            var toa = db.Toas.FirstOrDefault(t => t.MaToa == maToa);
            if (toa == null)
            {
                TempData["ErrorMessage"] = "Toa không tồn tại.";
                return RedirectToAction("XemToa");
            }

            ViewBag.loaiToas = db.LoaiToas.ToList();

            return View(toa);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CapNhatToa(FormCollection form)
        {
            string maToa = form["MaToa"];
            string loaiToa = form["LoaiToa"];
            int soToa = Convert.ToInt32(form["soToa"]);

            var toa = db.Toas.FirstOrDefault(t => t.MaToa == maToa);
            if (toa == null)
            {
                TempData["ErrorMessage"] = "Toa không tồn tại.";
                return RedirectToAction("XemToa", new { maTau = toa.MaTau });
            }
            // Cập nhật thông tin toa
            toa.MaLoaiToa = loaiToa;
            toa.SoToa = soToa;
            try
            {
                db.SubmitChanges();

                TempData["SuccessMessage"] = "Cập nhật toa thành công!";
                return RedirectToAction("XemToa", new { maTau = toa.MaTau });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("XemToa", new { maTau = toa.MaTau });
            }
        }



        public ActionResult XemToa(string maTau)
        {
            var tau = db.Taus.FirstOrDefault(t => t.MaTau == maTau);

            if (tau == null)
            {
                TempData["ErrorMessage"] = "Tàu không tồn tại.";
                return RedirectToAction("DanhSachTau");
            }

            var toaList = db.Toas
                            .Where(t => t.MaTau == maTau)
                            .Select(t => new
                            {
                                t.MaToa,
                                t.SoToa,
                                t.MaLoaiToa,
                                LoaiToa = t.LoaiToa.TenLoaiToa, 
                                t.LoaiToa.GiaMacDinh,
                                t.LoaiToa.CoDieuHoa
                            })
                            .ToList();

            var toaKhoangList = toaList.Select(toa => new ToaKhoangViewModel
            {
                MaToa = toa.MaToa,
                SoToa = toa.SoToa,
                MaLoaiToa = toa.MaLoaiToa,
                LoaiToa = toa.LoaiToa,
                GiaMacDinh = toa.GiaMacDinh,
                CoDieuHoa = toa.CoDieuHoa,
                KhoangList = db.Khoangs
                                .Where(k => k.MaToa == toa.MaToa)
                                .Select(k => new KhoangViewModel
                                {
                                    MaKhoang = k.MaKhoang,
                                    SoKhoang = k.SoKhoang,
                                    SoChoNgoiToiDa = k.SoChoNgoiToiDa ?? 0,
                                    SoChoNgoiConLai = k.SoChoNgoiConLai ?? 0
                                })
                                .ToList()
            }).OrderBy(t => t.SoToa).ToList();

            // Trả về view và truyền dữ liệu cần thiết

            var loaiToas = db.LoaiToas.ToList();
            ViewBag.loaiToas = loaiToas;

            return View(new XemToaViewModel
            {
                MaTau = tau.MaTau,
                TenTau = tau.TenTau,
                Toas = toaKhoangList
            });
        }

        [HttpPost]
        public ActionResult ThemKhoang(FormCollection form)
        {
            string maToa = form["MaToa"];
            string maLoaiKhoang = form["LoaiKhoang"];
            int soKhoang = Convert.ToInt32(form["SoKhoang"]);
            int soChoNgoiToiDa = Convert.ToInt32(form["SoChoNgoiToiDa"]);

            var toa = db.Toas.FirstOrDefault(t => t.MaToa == maToa);
            if (toa == null)
            {
                TempData["ErrorMessage"] = "Toa không tồn tại.";
                return RedirectToAction("XemToa", new { maTau = toa.MaTau });
            }

            var khoang = new Khoang
            {
                MaToa = maToa,
                SoKhoang = soKhoang,
                SoChoNgoiToiDa = soChoNgoiToiDa
            };

            db.Khoangs.InsertOnSubmit(khoang);
            try
            {
                db.SubmitChanges();
                TempData["SuccessMessage"] = "Thêm khoang thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
            }

            return RedirectToAction("XemToa", new { maTau = toa.MaTau });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CapNhatKhoang(FormCollection form)
        {
            string maKhoang = form["MaKhoang"];
            int soKhoang = Convert.ToInt32(form["SoKhoang"]);
            int soChoNgoi = Convert.ToInt32(form["SoChoNgoiToiDa"]);

            var khoang = db.Khoangs.FirstOrDefault(k => k.MaKhoang == maKhoang);
            if (khoang == null)
            {
                TempData["ErrorMessage"] = "Khoang không tồn tại.";
                return RedirectToAction("XemToa", new { maTau = khoang.Toa.MaTau });
            }
            // Cập nhật thông tin toa
            khoang.SoKhoang = soKhoang;
            khoang.SoChoNgoiToiDa = soChoNgoi;
            try
            {
                db.SubmitChanges();

                TempData["SuccessMessage"] = "Cập nhật khoang thành công!";
                return RedirectToAction("XemToa", new { maTau = khoang.Toa.MaTau });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("XemToa", new { maTau = khoang.Toa.MaTau });
            }
        }


        [HttpPost]
        public ActionResult XoaKhoang(string maKhoang)
        {
            var khoang = db.Khoangs.FirstOrDefault(k => k.MaKhoang == maKhoang);
            if (khoang != null)
            {
                try
                {
                    db.Khoangs.DeleteOnSubmit(khoang);
                    db.SubmitChanges();

                    TempData["SuccessMessage"] = "Xóa khoang thành công!";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = ex.Message;
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Khoang không tồn tại.";
            }

            return RedirectToAction("XemToa", new { khoang.Toa.MaTau });
        }

    }
}