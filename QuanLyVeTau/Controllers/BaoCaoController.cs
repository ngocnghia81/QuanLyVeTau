﻿using PagedList;
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
            var topKhachHang = db.Top3KhachHangs.ToList()
                .OrderByDescending(k => k.SoChuyenDi)
                .ThenByDescending(k => k.TongTienMua).Take(3);

            if (topKhachHang == null || !topKhachHang.Any())
            {
                Console.WriteLine("Không có dữ liệu Top 3 khách hàng!");
            }

            return PartialView("Top3KhachHangPartial", topKhachHang);
        }

        [CustomRoleAuthorizeAttribute("Quản lý, Giám đốc")]
        public ActionResult DanhSachTop3KhachHang()
        {
            var topKhachHang = db.Top3KhachHangs.ToList()
                .OrderByDescending(k => k.SoChuyenDi)
                .ThenByDescending(k => k.TongTienMua).Take(3);

            return View(topKhachHang);
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