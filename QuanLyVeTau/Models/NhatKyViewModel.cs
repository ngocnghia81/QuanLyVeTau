using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuanLyVeTau.Models
{
    public class NhatKyViewModel
    {
        public string MaNhatKy { get; set; }
        public string MaTau { get; set; }
        public string MaLichTrinh { get; set; }
        public DateTime NgayGio { get; set; }
        public string TrangThai { get; set; }
        public DateTime? ThoiGianHoanThanhDuKien { get; set; }
    }
}