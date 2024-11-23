using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace QuanLyVeTau.Models
{
    public class VeRepository
    {
        public HashSet<int> GetGheDaBan(string maKhoang,Dictionary<string,string> data)
        {
            var maNK = data["maNK"];
            var from = data["from"];
            var to = data["to"];

            var gheDaBan = new HashSet<int>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["QL_VETAUConnectionString3"].ConnectionString))
            {
                conn.Open();
                string query = "select STT_Ghe from dbo.LayVeTheoGaDiDen(@MaKhoang,@MaNK,@DiemDi,@DiemDen)";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MaKhoang", maKhoang);
                    cmd.Parameters.AddWithValue("@MaNK", maNK);
                    cmd.Parameters.AddWithValue("@DiemDi", from);
                    cmd.Parameters.AddWithValue("@DiemDen", to);
                    string sqlWithParams = string.Format("select STT_Ghe FROM dbo.GetVeTheoGaDiDen({0},{1},N'{2}',N'{3}')",
                                              maKhoang, maNK, from, to);

                    Debug.WriteLine("Câu truy vấn SQL: " + sqlWithParams);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            gheDaBan.Add(reader.GetInt32(0));
                        }
                    }
                }
            }
            return gheDaBan;
        }
    }
}