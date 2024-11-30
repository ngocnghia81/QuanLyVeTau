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

    public class BaoCaoDoanhThuViewModel
    {
        public string MaNhatKy { get; set; }
        public DateTime NgayLapHoaDon { get; set; }
        public int SoLuongVeBanRa { get; set; }
        public decimal DoanhThu { get; set; }
        public DateTime NgayGio { get; set; }
        public List<NhanVienBCViewModel> NhanVien { get; set; }
    }

    public class NhanVienBCViewModel
    {
        public string MaNhanVien { get; set; }
        public string TenNhanVien { get; set; }
        public string TenChucVu { get; set; }
    }

}