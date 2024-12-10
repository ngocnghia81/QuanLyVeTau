using PagedList;
using QuanLyVeTau.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common.CommandTrees.ExpressionBuilder;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace QuanLyVeTau.Controllers
{
    public class BaoCaoController : Controller
    {
        private readonly QuanLyVeTauDBDataContext db;
        private readonly string connectionString;

        public BaoCaoController()
        {
            connectionString = ConfigurationManager.ConnectionStrings["QL_VETAUConnectionString"].ConnectionString;
            db = new QuanLyVeTauDBDataContext(connectionString);
        }

        [CustomRoleAuthorizeAttribute("*")]
        public ActionResult Top3KhachHangPartial()
        {
            var topKhachHang = (from kh in db.KhachHangs
                                join hd in db.HoaDons on kh.MaKhach equals hd.MaKhach
                                join ve in db.Ves on hd.MaHoaDon equals ve.MaHoaDon
                                join nk in db.NhatKyTaus on ve.MaNhatKy equals nk.MaNhatKy
                                where nk.NgayGio >= new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1) && nk.NgayGio <= DateTime.Now
                                group new { kh, ve } by new { kh.MaKhach, kh.TenKhach } into grouped
                                select new
                                {
                                    MaKhach = grouped.Key.MaKhach,
                                    TenKhach = grouped.Key.TenKhach,
                                    SoChuyenDi = grouped.Count(),
                                    TongTienMua = grouped.Sum(x => x.ve.GiaVe)
                                })
                                .OrderByDescending(k => k.SoChuyenDi)
                                .ThenByDescending(k => k.TongTienMua)
                                .Take(3)
                                .ToList();

            if (topKhachHang == null || !topKhachHang.Any())
            {
                Console.WriteLine("Không có dữ liệu Top 3 khách hàng!");
            }

            var topKhachHangView = topKhachHang.Select(x => new Top3KH
            {
                TenKhach = x.TenKhach,
                SoChuyenDi = x.SoChuyenDi,
                TongTienMua = x.TongTienMua
            }).ToList();

            return PartialView("Top3KhachHangPartial", topKhachHangView);
        }



        [CustomRoleAuthorizeAttribute("Quản lý, Giám đốc")]
        public ActionResult DanhSachTop3KhachHang()
        {
            var topKhachHang = (from kh in db.KhachHangs
                                join hd in db.HoaDons on kh.MaKhach equals hd.MaKhach
                                join ve in db.Ves on hd.MaHoaDon equals ve.MaHoaDon
                                join nk in db.NhatKyTaus on ve.MaNhatKy equals nk.MaNhatKy
                                where nk.NgayGio >= new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1) && nk.NgayGio <= DateTime.Now
                                group new { kh, ve } by new { kh.MaKhach, kh.TenKhach } into grouped
                                select new
                                {
                                    MaKhach = grouped.Key.MaKhach,
                                    TenKhach = grouped.Key.TenKhach,
                                    SoChuyenDi = grouped.Count(),
                                    TongTienMua = grouped.Sum(x => x.ve.GiaVe)
                                })
                                .OrderByDescending(k => k.SoChuyenDi)
                                .ThenByDescending(k => k.TongTienMua)
                                .Take(3)
                                .ToList();

            if (topKhachHang == null || !topKhachHang.Any())
            {
                Console.WriteLine("Không có dữ liệu Top 3 khách hàng!");
            }

            var topKhachHangView = topKhachHang.Select(x => new Top3KH
            {
                MaKhach = x.MaKhach,
                TenKhach = x.TenKhach,
                SoChuyenDi = x.SoChuyenDi,
                TongTienMua = x.TongTienMua
            }).ToList();

            return View(topKhachHangView);
        }


        [CustomRoleAuthorizeAttribute("Quản lý, Giám đốc")]
        public ActionResult DoanhThuTheoNgay(DateTime? from, DateTime? to, int page = 1)
        {
            var query = db.Vw_BaoCaoDoanhThuTheoNgays.AsQueryable();

            if (from.HasValue)
            {
                query = query.Where(x => x.NgayLapHoaDon >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(x => x.NgayLapHoaDon <= to.Value);
            }

            int pageSize = 10;
            var result = query.OrderByDescending(d => d.NgayLapHoaDon)
                                .ThenByDescending(d => d.NgayGio)
                                .ToPagedList(page, pageSize);
            return View(result);
        }

    }

}