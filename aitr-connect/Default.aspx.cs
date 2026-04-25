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
            Response.Redirect(AppConstant.PageCatalog.strStaffLoginPage);
        }

        protected void btnRegister_Click(object sender, EventArgs e)
        {
            Response.Redirect(AppConstant.PageCatalog.strRegisterPage);
        }

        protected void btnSurvey_Click(object sender, EventArgs e)
        {
            // clear old sessions and create new respondent with a new session 
            Session.Clear();

            // update status of survey 
            Session[AppConstant.SessionNameList.strIsSurveyActive] = true;

            // set order of question index
            Session[AppConstant.SessionNameList.strQuestionIndex] = 1;

            Response.Redirect(AppConstant.PageCatalog.strSurveyPage);
        }
    }
}