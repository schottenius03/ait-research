using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace aitr_connect.Services
{
    public class SearchService
    {
        // Hämtar de frågor/kategorier som ska visas i sökfältet
        public DataTable GetSearchableCriteria(string connectionString)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                // Vi hämtar frågorna 1-12 som vi definierat i vår View
                string sql = "SELECT questionID, questionText FROM Question WHERE questionID <= 12";
                using (SqlDataAdapter adapter = new SqlDataAdapter(sql, conn))
                {
                    adapter.Fill(dt);
                }
            }
            return dt;
        }

        // Utför den dynamiska sökningen
        public DataTable GetFilteredRespondents(string connectionString, List<SqlParameter> parameters, string whereClause)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                // Vi behåller ORDER BY här för att garantera kravet om sortering på efternamn
                string sql = "SELECT * FROM V_RespondentReport WHERE 1=1 " + whereClause + " ORDER BY [Last Name] ASC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    // FIX: Vi lägger bara till parametrar om listan inte är null och innehåller något
                    if (parameters != null && parameters.Count > 0)
                    {
                        cmd.Parameters.AddRange(parameters.ToArray());
                    }

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        conn.Open();
                        adapter.Fill(dt);
                    }
                }
            }
            return dt;
        }

        public List<string> GetViewColumns(string connectionString)
        {
            List<string> columns = new List<string>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string sql = "SELECT TOP 0 * FROM V_RespondentReport";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SchemaOnly))
                    {
                        DataTable schemaTable = reader.GetSchemaTable();
                        foreach (DataRow row in schemaTable.Rows)
                        {
                            columns.Add(row["ColumnName"].ToString());
                        }
                    }
                }
            }
            return columns;
        }
    }
}