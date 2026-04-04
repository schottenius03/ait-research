using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace aitr_connect
{
    public partial class Search : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // tea - straw - connect - poke - get soda - consume
            String myConnectionString = "Data Source=SQL8012.site4now.net;Initial Catalog=db_9ab8b7_126dda14621;User Id=db_9ab8b7_126dda14621_admin;Password=DG5py76V;";

            SqlConnection myconn = new SqlConnection();
            myconn.ConnectionString = myConnectionString;

            try
            {
                myconn.Open();

                SqlCommand myCmd = new SqlCommand("SELECT * FROM TabUser", myconn);
                SqlDataReader reader = myCmd.ExecuteReader();

                DataTable dt = new DataTable();
                dt.Columns.Add("User ID", typeof(Int32));
                dt.Columns.Add("Username", typeof(String));
                dt.Columns.Add("Password", typeof(String));
                dt.Columns.Add("User Level", typeof(Int32));

                while (reader.Read())
                {
                    DataRow row = dt.NewRow();
                    row["User ID"] = reader["UID"];
                    row["Username"] = reader["UserName"];
                    row["Password"] = reader["Password"];
                    row["User Level"] = reader["UserLevel"];

                    dt.Rows.Add(row);
                }

                gvUser.DataSource = dt;
                gvUser.DataBind(); // bind gv to reader

                myconn.Close();
            }
            catch (InvalidOperationException ex)
            {
                lblTitle.Text = "Interal operation error!!! Contact admin";
            }

            catch (ConfigurationErrorsException ex)
            {
                lblTitle.Text = "Interal configuration error!!! Contact admin";
            }

            catch (SqlException ex)
            {
                lblTitle.Text = "Database general error!!! Try again later.";
            }

            catch (Exception ex)
            {
                lblTitle.Text = "General system error!!! Try again later.";
            }
        }
    }
}