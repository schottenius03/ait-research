using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace aitr_connect
{
    public partial class Register : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // get connection
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