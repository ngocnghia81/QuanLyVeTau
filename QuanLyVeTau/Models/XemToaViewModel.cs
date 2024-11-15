using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace QuanLyVeTau.Models
{
    public class XemToaViewModel
    {
        public string MaTau { get; set; }
        public string TenTau { get; set; }
        public List<ToaKhoangViewModel> Toas { get; set; }
    }

    public class ToaKhoangViewModel
    {
        public string MaToa { get; set; }
        public int SoToa { get; set; }
        public string MaLoaiToa { get; set; }
        public string LoaiToa { get; set; }
        public decimal GiaMacDinh { get; set; }
        public bool CoDieuHoa { get; set; }
        public List<KhoangViewModel> KhoangList { get; set; }
    }

    public class KhoangViewModel
    {
        public string MaKhoang { get; set; }
        public int SoKhoang { get; set; }
        public int SoChoNgoiToiDa { get; set; }
        public int SoChoNgoiConLai { get; set; }
    }

}
