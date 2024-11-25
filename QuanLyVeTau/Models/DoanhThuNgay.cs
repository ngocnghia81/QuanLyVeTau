using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuanLyVeTau.Models
{
    public class DoanhThuTheoNgay
    {
        public DateTime Ngay { get; set; }
        public decimal DoanhThu { get; set; }
        public List<HoaDon> HoaDons { get; set; }
    }

}