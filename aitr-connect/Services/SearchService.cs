using System;
using System.Data;
using System.Data.SqlClient;

namespace aitr_connect.Services
{
    public class SearchService
    {
        public DataTable GetAllRespondents(string connectionString)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string sql = "SELECT * FROM V_RespondentReport";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        conn.Open();
                        adapter.Fill(dt);
                    }
                }
            }
            return dt;
        }

        public DataTable GetFilterCriteria(string connectionString)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                // Hämtar t.ex. Kön, Ålder, Ort för filtrering
                string sql = "SELECT questionID, questionText FROM Question WHERE questionID IN (1, 2, 3)";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        conn.Open();
                        adapter.Fill(dt);
                    }
                }
            }
            return dt;
        }
    }
}