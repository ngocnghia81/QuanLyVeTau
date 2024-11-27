using System;
using System.Collections.Generic;


namespace QuanLyVeTau.Models
{
    public class PhanCongViewModel
    {
        public string MaPhanCong { get; set; }
        public string MaNhatKy { get; set; }
        public string MaLichTrinh { get; set; }
        public DateTime NgayGio { get; set; }
        public string TrangThai { get; set; }
        public List<NhanVien> NhanViens { get; set; }
        public List<string> ChucVus { get; set; }
        public bool ChuaPhanCong { get; set; }
    }

}