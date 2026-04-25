using System;
using System.Web.UI.WebControls;
using aitr_connect.Services;

namespace aitr_connect
{
    public partial class Register : PageBase
    {
        private RegisterService _registerService = new RegisterService();

        protected void Page_Init(object sender, EventArgs e)
        {
            if (!PageValid()) Response.Redirect(AppConstant.PageCatalog.strErrorPage);

            if (Session[AppConstant.SessionNameList.strRespondentID] == null)
            {
                int newID = _registerService.CreateNewRespondentWithSession(this.CurrentConnectionString, Request.UserHostAddress);
                Session[AppConstant.SessionNameList.strRespondentID] = newID;
            }

            if (!IsPostBack && Session[AppConstant.SessionNameList.strCurrentRegisterQuestionID] == null)
            {
                Session[AppConstant.SessionNameList.strCurrentRegisterQuestionID] = AppConstant.QuestionConfig.intFirstRegisterQuestionID;
            }
            RenderQuestion();
        }

        protected void btnNext_Click(object sender, EventArgs e)
        {
            ProcessSubmission(false);
        }

        protected void btnRegister_Click(object sender, EventArgs e)
        {
            ProcessSubmission(true);
        }
        protected void btnBackToDefault_Click(object sender, EventArgs e)
        {
            Response.Redirect(AppConstant.PageCatalog.strDefaultPage);
        }

        /// <summary>
        /// Validate and save 
        /// </summary>
        /// <param name="isFinalStep"></param>
        private void ProcessSubmission(bool isFinalStep)
        {
            try
            {
                int qID = Convert.ToInt32(Session[AppConstant.SessionNameList.strCurrentRegisterQuestionID]);
                int resID = Convert.ToInt32(Session[AppConstant.SessionNameList.strRespondentID]);

                TextBox tbx = (TextBox)phQuestionArea.FindControl("tbxAnswer");
                string answer = tbx?.Text ?? "";

                var validation = _registerService.ValidateUserSubmission(this.CurrentConnectionString, qID, answer);

                if (!validation.IsValid)
                {
                    lblErrorMessage.Text = validation.ErrorMessage;
                    lblErrorMessage.Visible = true;
                    RenderQuestion();
                    return;
                }

                // save data
                _registerService.UpdateRespondentData(this.CurrentConnectionString, resID, qID, answer);

                if (isFinalStep)
                {
                    // complete registration
                    if (_registerService.MarkAsRegistered(this.CurrentConnectionString, resID))
                    {
                        Session.Remove(AppConstant.SessionNameList.strCurrentRegisterQuestionID);
                        Response.Redirect(AppConstant.PageCatalog.strDefaultPage);
                    }
                }
                else
                {
                    // move to next question
                    Session[AppConstant.SessionNameList.strCurrentRegisterQuestionID] = qID + 1;
                    lblErrorMessage.Visible = false;
                    RenderQuestion();
                }
            }
            catch (Exception ex)
            {
                lblErrorMessage.Text = "Error: " + ex.Message;
                lblErrorMessage.Visible = true;
                RenderQuestion();
            }
        }

        /// <summary>
        /// update question and show the question and the correct button to go to next or register 
        /// </summary>
        private void RenderQuestion()
        {
            phQuestionArea.Controls.Clear();
            if (Session[AppConstant.SessionNameList.strCurrentRegisterQuestionID] == null) return;

            int qID = Convert.ToInt32(Session[AppConstant.SessionNameList.strCurrentRegisterQuestionID]);
            var question = _registerService.GetQuestionByID(this.CurrentConnectionString, qID);

            if (question != null)
            {
                lblTitle.Text = question.Text;
                phQuestionArea.Controls.Add(new TextBox { ID = "tbxAnswer", CssClass = "form-control" });

                // make sure isLastQuestion is true
                if (question.IsLastQuestion)
                {
                    btnNext.Visible = false;
                    btnRegister.Visible = true;
                }
                else
                {
                    btnNext.Visible = true;
                    btnRegister.Visible = false;
                }
            }
            else
            {
                // if we're outside index for some reason
                Response.Redirect(AppConstant.PageCatalog.strDefaultPage);
            }
        }
    }
}