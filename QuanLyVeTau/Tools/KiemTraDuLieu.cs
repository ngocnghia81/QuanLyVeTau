using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace QuanLyVeTau.Tools
{
    public static class KiemTraDuLieu
    {
        // Kiểm tra email hợp lệ
        public static bool KiemTraEmail(string email)
        {
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            return Regex.IsMatch(email, pattern);
        }

        // Kiểm tra số điện thoại hợp lệ của Việt Nam
        public static bool KiemTraSDT_VN(string sdt)
        {
            // Định dạng số điện thoại VN: bắt đầu bằng 0 và theo sau là 9-10 chữ số (tùy thuộc vào mạng)
            string pattern = @"^0[1-9][0-9]{8,9}$";
            return Regex.IsMatch(sdt, pattern);
        }

        // Kiểm tra chuỗi chỉ chứa các số
        public static bool KiemTraChuoiSo(string chuoi)
        {
            string pattern = @"^\d+$"; // Chuỗi chỉ chứa các số
            return Regex.IsMatch(chuoi, pattern);
        }
    }
}