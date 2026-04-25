using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

namespace aitr_connect.Services
{
    // Simplified class to store dropdown options
    public class ColumnOption
    {
        public string Text { get; set; }
        public string Value { get; set; }
    }

    public class ColumnMetadata
    {
        public string ColumnName { get; set; }
        public List<ColumnOption> Options { get; set; } = new List<ColumnOption>();
    }

    public class SearchService
    {
        /// <summary>
        /// Gets all column names from the report view to determine which filters to render.
        /// </summary>
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
                        DataTable schema = reader.GetSchemaTable();
                        foreach (DataRow row in schema.Rows)
                        {
                            columns.Add(row["ColumnName"].ToString());
                        }
                    }
                }
            }
            return columns;
        }

        public ColumnMetadata GetColumnMetadata(string connectionString, string columnName)
        {
            ColumnMetadata metadata = new ColumnMetadata { ColumnName = columnName };

            if (!AppConstant.SearchSettings.FilterableColumns.Contains(columnName))
                return metadata;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string stem = columnName.ToLower();
                if (stem.EndsWith("s") && stem.Length > 4)
                    stem = stem.Substring(0, stem.Length - 1);

                string sql = @"
            SELECT DISTINCT o.optionText 
            FROM [Option] o
            JOIN Question q ON o.questionID = q.questionID
            WHERE (q.questionText LIKE '%' + @colName + '%' 
               OR q.questionText LIKE '%' + @stem + '%')";

                // separating bank and bank services 
                if (columnName == "Bank Services")
                {
                    sql += " OR (q.questionText LIKE '%Services%' AND q.questionText LIKE '%Bank%')";
                }

                if (columnName == "Bank")
                {
                    sql += " AND q.questionText NOT LIKE '%Services%'";
                }

                sql += " ORDER BY o.optionText ASC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@colName", columnName);
                    cmd.Parameters.AddWithValue("@stem", stem);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string optText = reader["optionText"].ToString();

                            if (columnName == "Newspaper" && (optText.ToLower().Contains("section") || optText.ToLower().Contains("page")))
                                continue;

                            metadata.Options.Add(new ColumnOption { Text = optText, Value = optText });
                        }
                    }
                }
            }
            return metadata;
        }

        /// <summary>
        /// Executes the filtered search against the database view.
        /// </summary>
        public DataTable GetFilteredRespondents(string connectionString, List<SqlParameter> parameters, string whereClause)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string sql = "SELECT * FROM V_RespondentReport WHERE 1=1 " + whereClause;
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                            cmd.Parameters.Add(new SqlParameter(param.ParameterName, param.Value));
                    }
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    conn.Open();
                    adapter.Fill(dt);
                }
            }
            return dt;
        }
    }
}