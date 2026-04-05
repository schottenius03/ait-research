using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace aitr_connect
{
    public partial class Survey : System.Web.UI.Page
    {
        protected void Page_PreInit(object sender, EventArgs e)
        {
            Response.Write("Page_PreInit call<br />");
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            Response.Write("Page_Init call<br />");
        }

        protected void Page_InitComplete(object sender, EventArgs e)
        {
            Response.Write("Page_InitComplete call<br />");
        }

        protected void Page_PreLoad(object sender, EventArgs e)
        {
            Response.Write("Page_PreLoad call<br />");
        }

        protected void Page_LoadComplete(object sender, EventArgs e)
        {
            Response.Write("Page_LoadComplete call<br />");
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            Response.Write("Page_PreRender call<br />");
        }

        protected void Page_PreRenderComplete(object sender, EventArgs e)
        {
            Response.Write("Page_PreRenderComplete call<br />");
        }
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnBackToDefault_Click(object sender, EventArgs e)
        {
            Response.Redirect(AppConstant.PageCatalog.strDefaultPage);
        }
    }
}