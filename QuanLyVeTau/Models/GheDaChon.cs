using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuanLyVeTau.Models
{
    public class GheDaChon
    {
        public string MaKhoang { get; set; }
        public int Stt { get; set; }
        public decimal Gia { get; set; }
        public bool OneWay { get; set; }
    }
}