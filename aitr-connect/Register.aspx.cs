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
    public partial class Register : PageBase
    {
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
            }
            catch (Exception ex) {
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