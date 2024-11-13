using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuanLyVeTau.Models
{
    public class VeChiTietViewModel
    {
        public string MaVe { get; set; }
        public string TenKhachHang { get; set; }
        public string TenTau { get; set; }
        public string TenLoaiToa { get; set; }
        public string TenLichTrinh { get; set; }
        public string DiemDi { get; set; }
        public string DiaChiDiemDi { get; set; }
        public string DiemDen { get; set; }
        public string DiaChiDiemDen { get; set; }
        public DateTime ThoiGianKhoiHanh { get; set; }
        public DateTime ThoiGianDen { get; set; }
        public double KhoangCach { get; set; }
        public int SttGhe { get; set; }
        public int GiaVe { get; set; }
        public decimal? ThanhTien { get; set; }
        public DateTime? ThoiGianLapHoaDon { get; set; }
        public bool DaThuHoi { get; set; }
    }

}