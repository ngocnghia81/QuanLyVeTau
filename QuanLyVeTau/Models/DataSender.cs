using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuanLyVeTau.Models
{
    public class DataSender
    {
        public Dictionary<string,string> timVe { get; set; }
        public List<GheDaChon> dsGhe { get; set; }
        public int ThanhTien { get; set; }
        public string MaKM { get; set; }
        public string MaNK { get; set; }
        public string MaNKRereturned { get; set; }
    }
}