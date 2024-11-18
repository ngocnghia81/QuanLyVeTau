using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuanLyVeTau.Models
{
    public class ChiTietLichTrinhViewModel
    {
        public string MaChiTiet { get; set; }
        public int Stt_Ga { get; set; }
        public string GaTen { get; set; }
        public string GaDiaChi { get; set; }
        public TimeSpan ThoiGianDiChuyenTuTramTruoc { get; set; }
        public double KhoangCachTuTramTruoc { get; set; }
    }
}