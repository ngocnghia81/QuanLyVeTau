using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuanLyVeTau.Tools
{
    public static class CongCu
    {
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }
    }
}