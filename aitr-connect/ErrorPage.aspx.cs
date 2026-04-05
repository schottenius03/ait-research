using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace aitr_connect
{
    public partial class ErrorPageaspx : System.Web.UI.Page
    {

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session[AppConstant.SessionNameList.strErroMessage] != null)
            {
                lblErrorMessage.Text = Session[AppConstant.SessionNameList.strErroMessage].ToString();
            }
        }

        protected void btnBackToDefault_Click(object sender, EventArgs e)
        {
            Response.Redirect(AppConstant.PageCatalog.strDefaultPage);
        }
    }
}