using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace aitr_connect
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            Response.Redirect(AppConstant.PageCatalog.strSearchPage);
        }

        protected void btnRegister_Click(object sender, EventArgs e)
        {
            Response.Redirect(AppConstant.PageCatalog.strRegisterPage);
        }

        protected void btnSurvey_Click(object sender, EventArgs e)
        {
            Response.Redirect(AppConstant.PageCatalog.strSurveyPage);
        }
    }
}