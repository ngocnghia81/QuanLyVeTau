using PagedList;
using QuanLyVeTau.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;

namespace QuanLyVeTau.Controllers
{
    public class VeController : Controller
    {
        private readonly QuanLyVeTauDBDataContext db;
        private readonly string connectionString;
        public VeController()
        {
            connectionString = ConfigurationManager.ConnectionStrings["QL_VETAUConnectionString"].ConnectionString;
            db = new QuanLyVeTauDBDataContext(connectionString);
        }
        public ActionResult Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return View("DangNhap,Ve");
            }
            return View("TimVe");
        }

        public ActionResult TimVe()
        {
            List<Ga> Gas = db.Gas.ToList();
            List<PhanHoi> phanHois = db.PhanHois.ToList();
            ViewBag.Ratings = phanHois;
            ViewBag.Gas = Gas;
            return View();
        }

        [HttpPost]
        public ActionResult ChonVe(FormCollection form)
        {
            string from = form["from"];
            string to = form["to"];
            string date = form["date"];
            string returnDate = form["returnDate"];
            bool roundTrip = form["roundTrip"] == "true";

            ViewBag.from = from;
            ViewBag.to = to;
            ViewBag.date = date;
            ViewBag.KhuHoi = false;

            string sql = string.Format("SELECT * FROM LayTau('{0}', N'{1}', N'{2}')", date, from, to);
            ViewBag.sql = sql;
            DataTable dt = new DataTable();

            using (var connection = new SqlConnection(this.connectionString))
            {
                connection.Open();

                using (var adapter = new SqlDataAdapter(sql, connection))
                {
                    adapter.Fill(dt);
                }
            }
            if (form["roundTrip"] != null)
            {
                DataTable dtReturned = new DataTable();
                string sql2 = string.Format("SELECT * FROM LayTau('{0}', N'{1}', N'{2}')", form["returnDate"], to, from);
                ViewBag.KhuHoi = true;
                ViewBag.dateReturnd = form["returnDate"];
                ViewBag.dtReturned = dtReturned;
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (var adapter = new SqlDataAdapter(sql2, connection))
                    {
                        adapter.Fill(dtReturned);
                    }
                }


            }
            List<KhuyenMai> khuyenMai = db.KhuyenMais.Where(
                    t => t.SoLuongConLai > 0 &&
                    t.NgayBatDau <= DateTime.Now &&
                    t.NgayKetThuc > DateTime.Now
                ).ToList();
            ViewBag.KMs = khuyenMai;
            ViewBag.dt = dt;
            KhachHang kh = db.KhachHangs.First(t=>t.Email == User.Identity.Name);
            ViewBag.TenKH = kh.TenKhach;
            return View("KetQuaChonVe"); 
        }

        public ActionResult KetQuaChonVe()
        {

            return View();
        }

        public ActionResult HienThiToa(string maTau)
        {
            List<Toa> toas = db.Toas.Where(t=>t.MaTau == maTau).ToList();
            return PartialView("HienThiToa", toas);
        }

        public ActionResult HienThiKhoang(string maToa, string from,string to, string maNK,int oneway)
        {
            string sql = string.Format("SELECT * FROM LayKhoang('{0}') ORDER BY SoKhoang DESC", maToa);
            DataTable dt = new DataTable();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var adapter = new SqlDataAdapter(sql, connection))
                {
                    adapter.Fill(dt);
                    string name = dt.Rows[0]["SoToa"].ToString();
                    string dieuhoa = " không điều hoà";
                    if (dt.Rows[0]["DieuHoa"].ToString() == "1")
                        dieuhoa = " có điều hoà";
                    ViewBag.SoToa = name+dieuhoa;

                }
            }

            Dictionary<string, string> data = new Dictionary<string, string>();
            if (oneway == 0)
            {
                data.Add("from", from);
                data.Add("to", to);
            }
            else
            {
                data.Add("to", from);
                data.Add("from", to);
            }
            
            data.Add("maNK", maNK);

            var gheDaBanTheoKhoang = new Dictionary<string, HashSet<int>>();
            foreach (DataRow dr in dt.Rows)
            {
                string maKhoang = dr["MaKhoang"].ToString();
                VeRepository veRepository = new VeRepository();
                gheDaBanTheoKhoang[maKhoang] = veRepository.GetGheDaBan(maKhoang, data);
            }

            ViewBag.gheDaBanTheoKhoang = gheDaBanTheoKhoang;

            return PartialView("HienThiKhoang", dt);
        }


        //Admin
        public ActionResult DanhSachVe(bool? daThuHoi, string maTau = "", string maKhach = "", string maVe = "", string diemDi = "", string diemDen = "", int page = 1)
        {
            var ves = db.Ves.AsQueryable();

            if (daThuHoi.HasValue)
            {
                ves = ves.Where(v => v.DaThuHoi == daThuHoi);
            }

            if (!string.IsNullOrEmpty(maTau))
            {
                ves = ves.Where(v => v.Khoang.Toa.Tau.MaTau.ToLower().Contains(maTau));
            }

            if (!string.IsNullOrEmpty(maKhach))
            {
                ves = ves.Where(v => v.HoaDon.MaKhach.ToLower().Contains(maKhach.ToLower()));
            }

            if (!string.IsNullOrEmpty(maVe))
            {
                ves = ves.Where(v => v.MaVe.ToLower().Contains(maVe.ToLower()));
            }

            if (!string.IsNullOrEmpty(diemDi))
            {
                ves = ves.Where(v => v.ChiTietLichTrinh.Ga.DiaChi.ToLower().Contains(diemDi.ToLower()));
            }

            if (!string.IsNullOrEmpty(diemDen))
            {
                ves = ves.Where(v => v.ChiTietLichTrinh1.Ga.DiaChi.ToLower().Contains(diemDen.ToLower()));
            }

            ViewBag.ActiveLink = "manageTicketsLink";

            int pageSize = 12;  
            var result = ves.ToPagedList(page, pageSize); 

            return View(result); 
        }



        public ActionResult ChiTietVe(string maVe)
        {
            var ve = db.Ves
                       .FirstOrDefault(v => v.MaVe == maVe);

            if (ve == null)
            {
                return HttpNotFound();
            }

            var hoaDon = db.HoaDons
                           .FirstOrDefault(hd => hd.MaHoaDon == ve.MaHoaDon);

            var khachHang = db.KhachHangs
                              .FirstOrDefault(kh => kh.MaKhach == hoaDon.MaKhach);

            var khoang = db.Khoangs.FirstOrDefault(k => k.MaKhoang == ve.MaKhoang);

            var toa = db.Toas
                        .FirstOrDefault(t => t.MaToa == khoang.MaToa);

            var nhatKt = db.NhatKyTaus.FirstOrDefault(nk => nk.MaNhatKy == ve.MaNhatKy && nk.MaTau == toa.MaTau);
            Debug.WriteLine(nhatKt.ToString());
            var lichTrinh = db.LichTrinhTaus
                              .FirstOrDefault(lt => lt.MaLichTrinh == nhatKt.MaLichTrinh);

            var chiTietLichTrinhDi = db.ChiTietLichTrinhs
                                       .FirstOrDefault(ct => ct.MaChiTiet == ve.DiemDi);

            var chiTietLichTrinhDen = db.ChiTietLichTrinhs
                                        .FirstOrDefault(ct => ct.MaChiTiet == ve.DiemDen);

            var tau = db.Taus
                        .FirstOrDefault(t => t.MaTau == toa.MaTau);

            DateTime tgkh = nhatKt.NgayGio;
            DateTime tgDen;

            try
            {
                tgDen = LayTongThoiGianDiChuyen(nhatKt.MaNhatKy, tgkh, chiTietLichTrinhDi.MaGa, chiTietLichTrinhDen.MaGa);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi tính thời gian đến: " + ex.Message);
                return View("Error");
            }

            double kc = db.ChiTietLichTrinhs
                          .Where(ct => ct.MaLichTrinh == lichTrinh.MaLichTrinh &&
                                       ct.Stt_Ga >= chiTietLichTrinhDi.Stt_Ga &&
                                       ct.Stt_Ga <= chiTietLichTrinhDen.Stt_Ga)
                          .Sum(ct => ct.KhoangCachTuTramTruoc);

            var veChiTiet = new VeChiTietViewModel

            {
                MaVe = ve.MaVe,
                GiaVe = ve.GiaVe,
                TenKhachHang = khachHang?.TenKhach,
                TenTau = tau?.TenTau,
                TenLoaiToa = toa?.LoaiToa.TenLoaiToa + ((toa?.LoaiToa.CoDieuHoa ?? false) ? " - Điều hoà" : ""),
                TenLichTrinh = lichTrinh?.TenLichTrinh,
                DiemDi = chiTietLichTrinhDi?.MaGa,
                DiaChiDiemDi = db.Gas.FirstOrDefault(g => g.MaGa == chiTietLichTrinhDi.MaGa)?.DiaChi,
                DiemDen = chiTietLichTrinhDen?.MaGa,
                DiaChiDiemDen = db.Gas.FirstOrDefault(g => g.MaGa == chiTietLichTrinhDen.MaGa)?.DiaChi,
                ThoiGianKhoiHanh = tgkh,
                ThoiGianDen = tgDen,
                KhoangCach = kc,
                SttGhe = ve.Stt_Ghe,
                ThanhTien = hoaDon?.ThanhTien,
                ThoiGianLapHoaDon = hoaDon?.ThoiGianLapHoaDon,
                DaThuHoi = ve.DaThuHoi
            };

            return View(veChiTiet);
        }



        private DateTime LayTongThoiGianDiChuyen(string maNhatKy, DateTime thoiGianDi, string maGaDi, string maGaDen)
        {
            DataTable dtReturned = new DataTable();
            DateTime thoiGianDiChuyen = DateTime.MinValue;

            string sql2 = string.Format("SELECT dbo.TinhTongThoiGianDiChuyen('{0}', '{1}', '{2}', '{3}')",
                                        maNhatKy,
                                        thoiGianDi.ToString("yyyy-MM-dd HH:mm:ss"),
                                        maGaDi,
                                        maGaDen);

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var adapter = new SqlDataAdapter(sql2, connection))
                {
                    adapter.Fill(dtReturned);
                }
            }

            if (dtReturned.Rows.Count > 0)
            {
                thoiGianDiChuyen = Convert.ToDateTime(dtReturned.Rows[0][0]);
            }
            else
            {
                throw new Exception("Không tìm thấy dữ liệu thời gian di chuyển.");
            }

            return thoiGianDiChuyen;
        }

        [HttpPost]
        public ActionResult TaoVe([System.Web.Http.FromBody] DataSender data)
        {
            List<GheDaChon> dsGhe = data.dsGhe;
            Dictionary<string,string> timVe = data.timVe;
            int thanhTien = data.ThanhTien;
            string maKM = data.MaKM;
            string email = User.Identity.Name;
            string maNK = data.MaNK;
            string maNKReturned = data.MaNKRereturned;
            //Tạo hoá đơn
            string queryHoaDon = string.Format("exec dbo.TAOHOADON @email = '{0}', @MaKhuyenMai = {1}, @ThanhTien = '{2}', @ThoiGian = null", email, maKM, thanhTien);
            Debug.WriteLine(queryHoaDon);
            string maHoaDon = "";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string queryTaoMa = string.Format("select from dbo.TaoMa('HD',Getdate())");
                DataTable dt =  new DataTable();
                using (var adapter = new SqlDataAdapter(queryHoaDon, conn))
                {
                    // Điền dữ liệu vào DataTable
                    adapter.Fill(dt);
                    maHoaDon = dt.Rows[0][0].ToString();

                }
                
            }
            foreach (GheDaChon ghe in dsGhe)
            {
                string queryVe = "";
                if (ghe.OneWay)
                {
                     queryVe = string.Format("EXEC dbo.TAOVE @MaNhatKy = '{0}', @MaHoaDon = '{1}', @GiaVe = {2}, @MaKhoang = '{3}', @stt = {4}, @DiemDi = N'{5}', @DiemDen = N'{6}'",
              maNK, maHoaDon, ghe.Gia, ghe.MaKhoang, ghe.Stt, timVe["from"], timVe["to"]);

                }
                else
                {
                     queryVe = string.Format("EXEC dbo.TAOVE @MaNhatKy = '{0}', @MaHoaDon = '{1}', @GiaVe = {2}, @MaKhoang = '{3}', @stt = {4}, @DiemDi = N'{5}', @DiemDen = N'{6}'",
              maNKReturned, maHoaDon, ghe.Gia, ghe.MaKhoang, ghe.Stt, timVe["to"], timVe["from"]);
                }
                db.ExecuteCommand(queryVe);
            }

            return Json(new { success = true, message = "Dữ liệu đã được lưu thành công.", urlHoaDon = Url.Action("HoaDon","NguoiDung",new { mahoadon = maHoaDon}) });
        }                     
        [HttpPost]
        public JsonResult ThemHanhLy(string maVe, float khoiLuong)
        {
            try
            {
                var hanhLy = new HanhLy
                {
                    MaVe = maVe,
                    KhoiLuong = khoiLuong
                };

                db.HanhLies.InsertOnSubmit(hanhLy);  
                db.SubmitChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

    }
}