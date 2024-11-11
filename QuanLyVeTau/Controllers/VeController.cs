﻿using QuanLyVeTau.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QuanLyVeTau.Controllers
{
    public class VeController : Controller
    {
        QuanLyVeTauDBDataContext db;
        public VeController()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["QL_VETAUConnectionString1"].ConnectionString;
            db = new QuanLyVeTauDBDataContext(connectionString);
        }
        // GET: Ve
        public ActionResult Index()
        {
            return View();
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
            string connectionString = "Data Source=LAPTOP-SVSNGLVO\\SQLEXPRESS;Initial Catalog=QL_VETAU;Integrated Security=True;";

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
            if (form["roundTrip"] !=null)
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
            ViewBag.dt = dt;
            // Sau khi xử lý, có thể chuyển hướng tới View khác hoặc trả về View với dữ liệu đã xử lý
            return View("KetQuaChonVe"); // Tên của View bạn muốn hiển thị
        }

        public ActionResult KetQuaChonVe()
        {
            return View();
        }
    }

   
}