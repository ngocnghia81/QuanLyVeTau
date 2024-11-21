using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuanLyVeTau.Models
{
    public class NhanVienViewModel
    {
        public string MaNhanVien { get; set; }
        public string TenNhanVien { get; set; }
        public string Email { get; set; }
        public string SDT { get; set; }
        public string CCCD { get; set; }
        public int NamSinh { get; set; }
        public string VaiTro { get; set; }
        public bool DaXoa { get; set; }
        public string ChucVu {  get; set; }
        public string MoTa { get; set; }
        public double HeSoLuong { get; set; }
    }


}