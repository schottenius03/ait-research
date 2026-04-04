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
            String myConnectionString = ConfigurationManager.ConnectionStrings["KailingConnectionString"].ConnectionString;


            if (myConnectionString.ToUpper().Equals("DEV"))
            {
                myConnectionString = AppConstant.Connection.DevConnectionString;
            }
            else if (myConnectionString.ToUpper().Equals("TEST"))
            {
                myConnectionString = AppConstant.Connection.TestConnectionString;
            }
            else if (myConnectionString.ToUpper().Equals("PROD"))
            {
                myConnectionString = AppConstant.Connection.ProdConnectionString;
            }
            else
            {
                myConnectionString = "";
            }

            SqlConnection myconn = new SqlConnection();
            myconn.ConnectionString = myConnectionString;

            try
            {
                myconn.Open();

                SqlCommand myCmd = new SqlCommand("SELECT * FROM Respondent", myconn);
                SqlDataReader reader = myCmd.ExecuteReader();

                DataTable dt = new DataTable();
                dt.Columns.Add("Firstname", typeof(String));
                dt.Columns.Add("Surname", typeof(String));
                dt.Columns.Add("Date of Birth", typeof(DateTime));
                dt.Columns.Add("Phone Number", typeof(String));
                dt.Columns.Add("Email", typeof(String));

                while (reader.Read())
                {
                    DataRow row = dt.NewRow();
                    row["Firstname"] = reader["firstName"];
                    row["Surname"] = reader["lastName"];
                    row["Date of Birth"] = reader["dateOfBirth"]; 
                    row["Phone Number"] = reader["phoneNumber"];
                    row["Email"] = reader["email"];

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

        protected void btnBackToDefault_Click(object sender, EventArgs e)
        {
            Response.Redirect(AppConstant.PageCatalog.strDefaultPage);
        }
    }
}