using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuanLyVeTau.Models
{
    public class LichTrinhViewModel
    {
        public string MaLichTrinh { get; set; }
        public string TenLichTrinh {  set; get; }
        public string TrangThai { set; get; }
        public HashSet<NhatKyTau> NhatKyTaus { set; get; }
    }
}