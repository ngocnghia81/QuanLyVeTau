using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace QuanLyVeTau.Models
{
    public class VeRepository
    {
        public HashSet<int> GetGheDaBan(string maKhoang)
        {
            var gheDaBan = new HashSet<int>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["QL_VETAUConnectionString3"].ConnectionString))
            {
                conn.Open();
                string query = "SELECT STT_Ghe FROM Ve WHERE MaKhoang = @MaKhoang";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MaKhoang", maKhoang);
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