using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuanLyVeTau.Models
{
    public class ThongTinVe
    {
        public string MaVe { get; set; }
        public string TenTau { get; set; }
        public string TenLichTrinh { get; set; }
        public decimal GiaVe { get; set; }
        public string DiemDi { get; set; }
        public string DiemDen { get; set; }
        public bool DaThuHoi { get; set; }
    }

}