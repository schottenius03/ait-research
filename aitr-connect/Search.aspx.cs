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
    public partial class Search : PageBase
    {
        // run PageBase before getting all the objects 
        protected void Page_Init(object sender, EventArgs e)
        {
            if (!PageValid())
            {
                Response.Redirect(AppConstant.PageCatalog.strErrorPage);
            }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            // get enviroment from PageBase
            SqlConnection myconn = new SqlConnection(this.CurrentConnectionString);

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
            catch (Exception ex) // getting all exceptions 
            {
                // set errorMessage
                Session[AppConstant.SessionNameList.strErroMessage] = "An unexpected error occurred while loading data. Please try again later.";

                // redirect to ErrorPage
                Response.Redirect(AppConstant.PageCatalog.strErrorPage);
            }
        }

        protected void btnBackToDefault_Click(object sender, EventArgs e)
        {
            Response.Redirect(AppConstant.PageCatalog.strDefaultPage);
        }
    }
}