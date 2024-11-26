using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuanLyVeTau.Models
{
    public class BaoCaoHoaDonModel
    {
        public string MaHoaDon { get; set; }
        public string TenKhach { get; set; }
        public string MaKhach { get; set; }
        public decimal ThanhTien { get; set; }
        public DateTime ThoiGianLapHoaDon { get; set; }
    }
}