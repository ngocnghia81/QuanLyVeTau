
//﻿using PagedList;
using QuanLyVeTau.Models;
﻿using QuanLyVeTau.Models;
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
        QuanLyVeTauDBDataContext db;
        string connectionString = ConfigurationManager.ConnectionStrings["QL_VETAUConnectionString3"].ConnectionString;
        public VeController()
        {
            
            db = new QuanLyVeTauDBDataContext(connectionString);
        }
        // GET: Ve
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

            return View();
        }

        [HttpPost]
        public ActionResult ChonVe(FormCollection form)
        {
            // Lấy dữ liệu từ FormCollection
            string from = form["from"];
            string to = form["to"];
            string date = form["date"];
            string returnDate = form["returnDate"];
            bool roundTrip = form["roundTrip"] == "true";

            ViewBag.from = from;
            ViewBag.to = to;
            ViewBag.date = date;
            ViewBag.KhuHoi = false;
            // Chuỗi kết nối đến cơ sở dữ liệu của bạn
            string connectionString = ConfigurationManager.ConnectionStrings["QL_VETAUConnectionString3"].ConnectionString;

            // Tạo câu lệnh SQL để gọi hàm LayTau
            string sql = string.Format("SELECT * FROM LayTau('{0}', N'{1}', N'{2}')", date, from, to);
            ViewBag.sql = sql;
            // Tạo một DataTable để chứa kết quả trả về
            DataTable dt = new DataTable();

            // Sử dụng SqlConnection và SqlDataAdapter để thực thi truy vấn
            using (var connection = new SqlConnection(connectionString))
            {
                // Mở kết nối đến cơ sở dữ liệu
                connection.Open();

                // Tạo SqlDataAdapter để thực thi truy vấn và điền kết quả vào DataTable
                using (var adapter = new SqlDataAdapter(sql, connection))
                {
                    // Điền dữ liệu vào DataTable
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
                    // Mở kết nối đến cơ sở dữ liệu
                    connection.Open();

                    // Tạo SqlDataAdapter để thực thi truy vấn và điền kết quả vào DataTable
                    using (var adapter = new SqlDataAdapter(sql2, connection))
                    {
                        // Điền dữ liệu vào DataTable
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
            // Sau khi xử lý, có thể chuyển hướng tới View khác hoặc trả về View với dữ liệu đã xử lý
            return View("KetQuaChonVe"); // Tên của View bạn muốn hiển thị
        }

        public ActionResult KetQuaChonVe()
        {

            return View();
        }

        public ActionResult HienThiToa(string maTau)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["QL_VETAUConnectionString3"].ConnectionString;

            // Tạo câu lệnh SQL để gọi hàm LayTau
            string sql = string.Format("SELECT * FROM LayToa('{0}') ORDER BY SoToa DESC", maTau);
            // Tạo một DataTable để chứa kết quả trả về
            DataTable dt = new DataTable();

            // Sử dụng SqlConnection và SqlDataAdapter để thực thi truy vấn
            using (var connection = new SqlConnection(connectionString))
            {
                // Mở kết nối đến cơ sở dữ liệu
                connection.Open();

                // Tạo SqlDataAdapter để thực thi truy vấn và điền kết quả vào DataTable
                using (var adapter = new SqlDataAdapter(sql, connection))
                {
                    // Điền dữ liệu vào DataTable
                    adapter.Fill(dt);

                }
            }
            return PartialView("HienThiToa", dt);
        }

        public ActionResult HienThiKhoang(string maToa, string from,string to, string maNK,int oneway)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["QL_VETAUConnectionString3"].ConnectionString;
            // Tạo câu lệnh SQL để gọi hàm LayTau
            string sql = string.Format("SELECT * FROM LayKhoang('{0}') ORDER BY SoKhoang DESC", maToa);
            // Tạo một DataTable để chứa kết quả trả về
            DataTable dt = new DataTable();

            // Sử dụng SqlConnection và SqlDataAdapter để thực thi truy vấn
            using (var connection = new SqlConnection(connectionString))
            {
                // Mở kết nối đến cơ sở dữ liệu
                connection.Open();

                // Tạo SqlDataAdapter để thực thi truy vấn và điền kết quả vào DataTable
                using (var adapter = new SqlDataAdapter(sql, connection))
                {
                    // Điền dữ liệu vào DataTable
                    adapter.Fill(dt);
                    ViewBag.SoToa = dt.Rows[0]["SoToa"];

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
        //public ActionResult DanhSachVe(bool? daThuHoi, string maTau = "", string maKhach = "", string maVe = "", string diemDi = "", string diemDen = "", int page = 1)
        //{
        //    var ves = db.Ves.AsQueryable();

        //    if (daThuHoi.HasValue)
        //    {
        //        ves = ves.Where(v => v.DaThuHoi == daThuHoi);
        //    }

        //    if (!string.IsNullOrEmpty(maTau))
        //    {
        //        ves = ves.Where(v => v.Khoang.Toa.Tau.MaTau.ToLower().Contains(maTau));
        //    }

        //    if (!string.IsNullOrEmpty(maKhach))
        //    {
        //        ves = ves.Where(v => v.HoaDon.MaKhach.ToLower().Contains(maKhach.ToLower()));
        //    }

        //    // Lọc theo mã vé
        //    if (!string.IsNullOrEmpty(maVe))
        //    {
        //        ves = ves.Where(v => v.MaVe.ToLower().Contains(maVe.ToLower()));
        //    }

        //    // Lọc theo điểm đi
        //    if (!string.IsNullOrEmpty(diemDi))
        //    {
        //        ves = ves.Where(v => v.ChiTietLichTrinh.Ga.DiaChi.ToLower().Contains(diemDi.ToLower()));
        //    }

        //    // Lọc theo điểm đến
        //    if (!string.IsNullOrEmpty(diemDen))
        //    {
        //        ves = ves.Where(v => v.ChiTietLichTrinh1.Ga.DiaChi.ToLower().Contains(diemDen.ToLower()));
        //    }

        //    // Tạo ViewBag cho liên kết của menu
        //    ViewBag.ActiveLink = "manageTicketsLink";

        //    // Phân trang
        //    int pageSize = 12;  // Số vé hiển thị trên mỗi trang
        //    var result = ves.ToPagedList(page, pageSize);  // Phân trang danh sách vé

        //    // Trả về view với danh sách vé đã phân trang
        //    return View(result);  // Trả về IPagedList<Ve> cho View
        //}



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

            var nhatKt = db.NhatKyTaus.FirstOrDefault(nk => nk.MaNhatKy == ve.MaNhatKy);

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
                TenLoaiToa = toa?.MaLoaiToa,
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
            DateTime thoiGianDiChuyen = DateTime.MinValue; // Khởi tạo giá trị mặc định nếu không có dữ liệu

            // Chuỗi truy vấn để gọi hàm SQL với các tham số truyền vào
            string sql2 = string.Format("SELECT dbo.TinhTongThoiGianDiChuyen('{0}', '{1}', '{2}', '{3}')",
                                        maNhatKy,
                                        thoiGianDi.ToString("yyyy-MM-dd HH:mm:ss"),
                                        maGaDi,
                                        maGaDen);

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["QL_VETAUConnectionString1"].ConnectionString))
            {
                // Mở kết nối đến cơ sở dữ liệu
                connection.Open();

                // Tạo SqlDataAdapter để thực thi truy vấn và điền kết quả vào DataTable
                using (var adapter = new SqlDataAdapter(sql2, connection))
                {
                    // Điền dữ liệu vào DataTable
                    adapter.Fill(dtReturned);
                }
            }

            // Kiểm tra nếu DataTable có ít nhất một hàng kết quả
            if (dtReturned.Rows.Count > 0)
            {
                // Giả sử kết quả trả về chứa thời gian ở cột đầu tiên (cột index 0)
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


    }
}