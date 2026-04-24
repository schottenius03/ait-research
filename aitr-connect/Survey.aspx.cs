using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using aitr_connect.Services;

namespace aitr_connect
{
    public partial class Survey : PageBase
    {
        // create instance of the service 
        private SurveyService surveyService = new SurveyService();

        protected void Page_Init(object sender, EventArgs e)
        {
            // verify session 
            if (!surveyService.IsSurveyAccessValid(Session[AppConstant.SessionNameList.strIsSurveyActive]))
            {
                Response.Redirect(AppConstant.PageCatalog.strDefaultPage);
                return;
            }

            // verify DB connection 
            if (!PageValid())
            {
                Response.Redirect(AppConstant.PageCatalog.strErrorPage);
                return;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!PageValid() || Session[AppConstant.SessionNameList.strQuestionIndex] == null)
            {
                Response.Redirect(AppConstant.PageCatalog.strDefaultPage);
                return;
            }

            int currentOrder = Convert.ToInt32(Session[AppConstant.SessionNameList.strQuestionIndex]);

            // if survey is finished
            if (currentOrder == -1)
            {
                Session.Remove(AppConstant.SessionNameList.strIsSurveyActive);
                Response.Redirect(AppConstant.PageCatalog.strDefaultPage);
                return;
            }

            // Screate session 
            if (!IsPostBack && Session[AppConstant.SessionNameList.strSessionID] == null)
            {
                var result = surveyService.CreateNewSurveySession(this.CurrentConnectionString, Request.UserHostAddress);
                Session[AppConstant.SessionNameList.strRespondentID] = result.RespondentID;
                Session[AppConstant.SessionNameList.strSessionID] = result.SessionID;
            }

            RenderQuestion(currentOrder);
        }

        protected void btnNextQuestion_Click(object sender, EventArgs e)
        {
            if (ValidateSelections())
            {
                SaveRespondentAnswer();
                MoveToNextQuestion();
            }
        }

        protected void btnSkip_Click(object sender, EventArgs e)
        {
            if (ValidateSelections())
            {
                MoveToNextQuestion();
            }
        }

        protected void btnBackToDefault_Click(object sender, EventArgs e)
        {
            // delete all sessions 
            Session.Abandon();

            // redirect to default page
            Response.Redirect(AppConstant.PageCatalog.strDefaultPage);
        }


        /// <summary>
        /// Checks if the user's selection meets the min and max requirements from the database.
        /// </summary>
        // Inuti ValidateSelections i Survey.aspx.cs
        private bool ValidateSelections()
        {
            int questionID = Convert.ToInt32(Session[AppConstant.SessionNameList.strQuestionID]);
            Control ctl = phQuestionArea.FindControl("ctlOptions");

            int count = 0;
            string textAnswer = null;

            // collect data from controls
            if (ctl is RadioButtonList rbl)
            {
                if (!string.IsNullOrEmpty(rbl.SelectedValue)) count = 1;
            }
            else if (ctl is CheckBoxList cbl)
            {
                foreach (ListItem item in cbl.Items) if (item.Selected) count++;
            }
            else if (ctl is DropDownList ddl)
            {
                if (ddl.SelectedValue != "0") count = 1;
            }
            else if (ctl is TextBox txt)
            {
                textAnswer = txt.Text;
                if (!string.IsNullOrWhiteSpace(textAnswer)) count = 1;
            }

            var result = surveyService.ValidateUserSubmission(this.CurrentConnectionString, questionID, count, textAnswer);

            if (!result.IsValid)
            {
                lblErrorMessage.Text = result.ErrorMessage;
                lblErrorMessage.Visible = true;
                return false;
            }

            lblErrorMessage.Visible = false;
            return true;
        }

        /// <summary>
        /// Reads values from UI controls and sends them to the Service layer for storage.
        /// </summary>
        private void SaveRespondentAnswer()
        {
            int sessionID = Convert.ToInt32(Session[AppConstant.SessionNameList.strSessionID]);
            int questionID = Convert.ToInt32(Session[AppConstant.SessionNameList.strQuestionID]);
            string type = Session[AppConstant.SessionNameList.strQuestionType].ToString();
            Control ctl = phQuestionArea.FindControl("ctlOptions");

            if (ctl == null) return;

            // get logic from values
            switch (type)
            {
                case "RadioButton":
                    var rbl = (RadioButtonList)ctl;
                    if (!string.IsNullOrEmpty(rbl.SelectedValue))
                        surveyService.SaveAnswer(this.CurrentConnectionString, sessionID, questionID, Convert.ToInt32(rbl.SelectedValue), null);
                    break;
                case "DropDown":
                    var ddl = (DropDownList)ctl;
                    if (ddl.SelectedValue != "0")
                        surveyService.SaveAnswer(this.CurrentConnectionString, sessionID, questionID, Convert.ToInt32(ddl.SelectedValue), null);
                    break;
                case "CheckBox":
                    var cbl = (CheckBoxList)ctl;
                    foreach (ListItem item in cbl.Items)
                        if (item.Selected)
                            surveyService.SaveAnswer(this.CurrentConnectionString, sessionID, questionID, Convert.ToInt32(item.Value), null);
                    break;
                case string t when t.StartsWith("TextBox"):
                    var txt = (TextBox)ctl;
                    if (!string.IsNullOrWhiteSpace(txt.Text))
                    {
                        surveyService.SaveAnswer(this.CurrentConnectionString, sessionID, questionID, null, txt.Text);
                    }
                    break;
                default:
                    throw new NotSupportedException($"Question type '{type}' is not supported by the system yet.");
            }
        }

        /// <summary>
        /// Handles navigation to the next main question by updating session and reloading the page.
        /// </summary>
        private void MoveToNextQuestion()
        {
            try
            {
                // get current question
                int currentOrder = Convert.ToInt32(Session[AppConstant.SessionNameList.strQuestionIndex]);

                // get next question that is not a sub question
                int nextOrder = surveyService.GetNextMainQuestionOrder(this.CurrentConnectionString, currentOrder, 1);

                // update current index
                Session[AppConstant.SessionNameList.strQuestionIndex] = nextOrder;

                // reload page to show updated question
                Response.Redirect(Request.RawUrl, false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (Exception ex)
            {
                Session[AppConstant.SessionNameList.strErroMessage] = "Navigation error: " + ex.Message;
                Response.Redirect(AppConstant.PageCatalog.strErrorPage);
            }
        }

        private void RenderQuestion(int currentOrder)
        {
            lblErrorMessage.Text = "";

            // get current question
            var currentQ = surveyService.GetQuestionByOrder(this.CurrentConnectionString, currentOrder, 1);

            if (currentQ != null)
            {
                // save to sessions
                Session[AppConstant.SessionNameList.strQuestionID] = currentQ.ID;
                Session[AppConstant.SessionNameList.strQuestionType] = currentQ.Type;

                lblQuestionNumber.Text = currentQ.ID.ToString();

                // clear text
                phQuestionArea.Controls.Clear();
                phQuestionArea.Controls.Add(new Label { ID = "lblQuestion", Text = currentQ.Text });
                phQuestionArea.Controls.Add(new LiteralControl("<br /><br />"));

                var options = surveyService.GetOptionsByQuestionID(this.CurrentConnectionString, currentQ.ID);

                switch (currentQ.Type)
                {
                    case "RadioButton":
                        RadioButtonList rbl = new RadioButtonList { ID = "ctlOptions", CssClass = "survey-rbl" };
                        foreach (var opt in options) rbl.Items.Add(new ListItem(opt.OptionText, opt.OptionID.ToString()));
                        phQuestionArea.Controls.Add(rbl);
                        break;

                    case "CheckBox":
                        CheckBoxList cbl = new CheckBoxList { ID = "ctlOptions", CssClass = "survey-cbl" };
                        foreach (var opt in options) cbl.Items.Add(new ListItem(opt.OptionText, opt.OptionID.ToString()));
                        phQuestionArea.Controls.Add(cbl);
                        break;

                    case "DropDown":
                        DropDownList ddl = new DropDownList { ID = "ctlOptions", CssClass = "survey-ddl" };
                        ddl.Items.Add(new ListItem("-- Select an option --", "0"));
                        foreach (var opt in options) ddl.Items.Add(new ListItem(opt.OptionText, opt.OptionID.ToString()));
                        phQuestionArea.Controls.Add(ddl);
                        break;

                    case string t when t.StartsWith("TextBox"):
                        TextBox txt = new TextBox { ID = "ctlOptions", TextMode = TextBoxMode.MultiLine, Rows = 4, CssClass = "form-control" };
                        phQuestionArea.Controls.Add(txt);
                        break;
                }
            }
        }
    }
}