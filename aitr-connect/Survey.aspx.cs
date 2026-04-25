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
            try
            {
                // check if page is valid and index exists
                if (!PageValid() || Session[AppConstant.SessionNameList.strQuestionIndex] == null)
                {
                    Response.Redirect(AppConstant.PageCatalog.strDefaultPage, false);
                    Context.ApplicationInstance.CompleteRequest();
                    return;
                }

                int currentOrder = Convert.ToInt32(Session[AppConstant.SessionNameList.strQuestionIndex]);

                // if survey is finished
                if (currentOrder == -1)
                {
                    if (Session[AppConstant.SessionNameList.strSessionID] != null)
                    {
                        int sessionID = Convert.ToInt32(Session[AppConstant.SessionNameList.strSessionID]);
                        surveyService.CompleteSession(this.CurrentConnectionString, sessionID);
                    }

                    Session.Remove(AppConstant.SessionNameList.strIsSurveyActive);
                    Response.Redirect(AppConstant.PageCatalog.strDefaultPage, false);
                    Context.ApplicationInstance.CompleteRequest();
                    return;
                }

                // create session for new respondent
                if (!IsPostBack && Session[AppConstant.SessionNameList.strSessionID] == null)
                {
                    var result = surveyService.CreateNewSurveySession(this.CurrentConnectionString, Request.UserHostAddress);
                    Session[AppConstant.SessionNameList.strRespondentID] = result.RespondentID;
                    Session[AppConstant.SessionNameList.strSessionID] = result.SessionID;
                }

                // attempt to render the question
                RenderQuestion(currentOrder);
            }
            catch (Exception ex)
            {
                // handle any unexpected errors during load or rendering
                HandleNavigationError(ex);
            }
        }

        protected void btnNextQuestion_Click(object sender, EventArgs e)
        {
            if (ValidateSelections())
            {
                SaveRespondentAnswer();
                MoveToNextQuestion(ignoreRules: false);
            }
        }

        protected void btnSkip_Click(object sender, EventArgs e)
        {
            // validate even if textbox is empty
            if (ValidateSelections())
            {
                MoveToNextQuestion(ignoreRules: true);
            }
        }

        protected void btnBackToDefault_Click(object sender, EventArgs e)
        {
            // get sessionID 
            if (Session[AppConstant.SessionNameList.strSessionID] != null)
            {
                int sessionID = Convert.ToInt32(Session[AppConstant.SessionNameList.strSessionID]);

                // mark complete in DB
                surveyService.CompleteSession(this.CurrentConnectionString, sessionID);
            }

            // delete all sessions
            Session.Abandon();

            Response.Redirect(AppConstant.PageCatalog.strDefaultPage);
        }


        /// <summary>
        /// Checks if the user's selection meets the min and max requirements from the database.
        /// </summary>
        /// <returns></returns>
        private bool ValidateSelections()
        {
            int questionID = Convert.ToInt32(Session[AppConstant.SessionNameList.strQuestionID]);
            Control ctl = phQuestionArea.FindControl("ctlOptions");

            int count = 0;
            string textAnswer = null;

            // 
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
            try
            {
                // Verify session exists
                if (Session[AppConstant.SessionNameList.strSessionID] == null ||
                    Session[AppConstant.SessionNameList.strQuestionID] == null)
                {
                    throw new Exception("Session has expired. Please restart the survey.");
                }

                int sessionID = Convert.ToInt32(Session[AppConstant.SessionNameList.strSessionID]);
                int questionID = Convert.ToInt32(Session[AppConstant.SessionNameList.strQuestionID]);
                string type = Session[AppConstant.SessionNameList.strQuestionType].ToString();
                Control ctl = phQuestionArea.FindControl("ctlOptions");

                if (ctl == null) return;

                // get logic from values
                switch (type)
                {
                    case AppConstant.QuestionTypes.RadioButton:
                    case AppConstant.QuestionTypes.RadioButtonRegister:
                        var rbl = ctl as RadioButtonList;
                        if (rbl != null && !string.IsNullOrEmpty(rbl.SelectedValue))
                            surveyService.SaveAnswer(this.CurrentConnectionString, sessionID, questionID, Convert.ToInt32(rbl.SelectedValue), null);
                        break;
                    case AppConstant.QuestionTypes.DropDown:
                        var ddl = ctl as DropDownList;
                        if (ddl != null && ddl.SelectedValue != "0")
                            surveyService.SaveAnswer(this.CurrentConnectionString, sessionID, questionID, Convert.ToInt32(ddl.SelectedValue), null);
                        break;
                    case string t when t.StartsWith(AppConstant.QuestionTypes.CheckBox):
                        var cbl = ctl as CheckBoxList;
                        if (cbl != null)
                        {
                            foreach (ListItem item in cbl.Items)
                                if (item.Selected)
                                    surveyService.SaveAnswer(this.CurrentConnectionString, sessionID, questionID, Convert.ToInt32(item.Value), null);
                        }
                        break;
                    case string t when t.StartsWith(AppConstant.QuestionTypes.TextBoxAlpha.Split('_')[0]):
                        var txt = ctl as TextBox;
                        if (txt != null && !string.IsNullOrWhiteSpace(txt.Text))
                        {
                            surveyService.SaveAnswer(this.CurrentConnectionString, sessionID, questionID, null, txt.Text);
                        }
                        break;
                    default:
                        System.Diagnostics.Debug.WriteLine($"Okänd frågetyp: {type}");
                        break;
                }
            }
            catch (Exception ex)
            {
                // Use the shared error handler
                HandleNavigationError(ex);
            }
        }

        /// <summary>
        /// Handles navigation to the next main question by updating session and reloading the page.
        /// </summary>
        /// <param name="ignoreRules"></param>
        /// <summary>
        /// Handles navigation to the next main question by updating session and reloading the page.
        /// </summary>
        /// <param name="ignoreRules"></param>
        private void MoveToNextQuestion(bool ignoreRules = false)
        {
            try
            {
                int currentOrder = Convert.ToInt32(Session[AppConstant.SessionNameList.strQuestionIndex]);
                string currentType = Session[AppConstant.SessionNameList.strQuestionType]?.ToString();

                if (currentType == AppConstant.QuestionTypes.RadioButtonRegister)
                {
                    Control ctl = phQuestionArea.FindControl("ctlOptions");
                    RadioButtonList rbl = ctl as RadioButtonList;

                    if (rbl != null && !string.IsNullOrEmpty(rbl.SelectedValue))
                    {
                        int selectedOptionID = Convert.ToInt32(rbl.SelectedValue);
                        int sessionID = Convert.ToInt32(Session[AppConstant.SessionNameList.strSessionID]);

                        if (selectedOptionID == AppConstant.QuestionTypes.RegisterYesOptionID)
                        {
                            // respondent want to register
                            surveyService.UpdateAnonymousStatus(this.CurrentConnectionString, sessionID, false);

                            Response.Redirect(AppConstant.PageCatalog.strRegisterPage, false);
                        }
                        else
                        {
                            // respondent don't want to register 
                            surveyService.UpdateAnonymousStatus(this.CurrentConnectionString, sessionID, true);

                            if (Session[AppConstant.SessionNameList.strSessionID] != null)
                            {
                                surveyService.CompleteSession(this.CurrentConnectionString, sessionID);
                            }

                            Session.Remove(AppConstant.SessionNameList.strIsSurveyActive);
                            Response.Redirect(AppConstant.PageCatalog.strDefaultPage, false);
                        }

                        Context.ApplicationInstance.CompleteRequest();
                        return;
                    }
                }

                // create list to store all sub questions
                List<int> triggeredOrders = new List<int>();

                if (!ignoreRules)
                {
                    Control ctl = phQuestionArea.FindControl("ctlOptions");
                    if (ctl is CheckBoxList cbl)
                    {
                        foreach (ListItem item in cbl.Items)
                        {
                            if (item.Selected)
                            {
                                // Uppdaterad: använder this.CurrentSurveyID istället för hårdkodad 1
                                int? order = surveyService.GetSubQuestionOrder(this.CurrentConnectionString, Convert.ToInt32(item.Value), this.CurrentSurveyID);

                                // add to list if not already exist
                                if (order.HasValue && !triggeredOrders.Contains(order.Value))
                                {
                                    triggeredOrders.Add(order.Value);
                                }
                            }
                        }
                    }
                    // handle radiobutton/dropdwon only trigger once
                    else if (ctl is ListControl list && !string.IsNullOrEmpty(list.SelectedValue))
                    {
                        int? order = surveyService.GetSubQuestionOrder(this.CurrentConnectionString, Convert.ToInt32(list.SelectedValue), this.CurrentSurveyID);
                        if (order.HasValue) triggeredOrders.Add(order.Value);
                    }
                }

                if (triggeredOrders.Count > 0)
                {
                    // sort saved triggered question in order 
                    triggeredOrders.Sort();

                    // save the rest to a list if any ese 
                    int firstNext = triggeredOrders[0];
                    triggeredOrders.RemoveAt(0);

                    // only save in session if any triggers are left
                    Session["QueuedQuestions"] = triggeredOrders.Count > 0 ? triggeredOrders : null;

                    ExecuteNavigation(firstNext);
                }
                else
                {
                    // if no triggers check if any are in the que 
                    List<int> queue = Session["QueuedQuestions"] as List<int>;

                    if (queue != null && queue.Count > 0)
                    {
                        int nextFromQueue = queue[0];
                        queue.RemoveAt(0);

                        // update que 
                        Session["QueuedQuestions"] = queue.Count > 0 ? queue : null;

                        ExecuteNavigation(nextFromQueue);
                    }
                    else
                    {
                        // go to next parent main question
                        int nextOrder = surveyService.GetNextMainQuestionOrder(this.CurrentConnectionString, currentOrder, this.CurrentSurveyID);
                        ExecuteNavigation(nextOrder);
                    }
                }
            }
            catch (Exception ex)
            {
                HandleNavigationError(ex);
            }
        }

        /// <summary>
        /// redirect to next question 
        /// </summary>
        /// <param name="nextOrder"></param>
        private void ExecuteNavigation(int nextOrder)
        {
            // save index to session
            Session[AppConstant.SessionNameList.strQuestionIndex] = nextOrder;

            // reload page
            Response.Redirect(Request.RawUrl, false);
            Context.ApplicationInstance.CompleteRequest();
        }

        /// <summary>
        /// handle navigation errrors
        /// </summary>
        /// <param name="ex"></param>
        private void HandleNavigationError(Exception ex)
        {
            // If the error is just the redirect itself, do nothing
            if (ex is System.Threading.ThreadAbortException) return;

            Session[AppConstant.SessionNameList.strErroMessage] = "Navigation error: " + ex.Message;
            Response.Redirect(AppConstant.PageCatalog.strErrorPage, false); // false prevent crash
            Context.ApplicationInstance.CompleteRequest();
        }

        /// <summary>
        /// renders the question based on configuration
        /// </summary>
        /// <param name="currentOrder"></param>
        private void RenderQuestion(int currentOrder)
        {
            lblErrorMessage.Text = "";
            // Get current question using the DDA structure from PageBase
            var currentQ = surveyService.GetQuestionByOrder(this.CurrentConnectionString, currentOrder, this.CurrentSurveyID);

            if (currentQ != null)
            {
                // save to sessions
                Session[AppConstant.SessionNameList.strQuestionID] = currentQ.ID;
                Session[AppConstant.SessionNameList.strQuestionType] = currentQ.Type;

                lblQuestionNumber.Text = currentQ.ID.ToString();

                // clear and render new controls
                phQuestionArea.Controls.Clear();
                phQuestionArea.Controls.Add(new Label { ID = "lblQuestion", Text = currentQ.Text });
                phQuestionArea.Controls.Add(new LiteralControl("<br /><br />"));

                var options = surveyService.GetOptionsByQuestionID(this.CurrentConnectionString, currentQ.ID);

                switch (currentQ.Type)
                {
                    case AppConstant.QuestionTypes.RadioButton:
                    case AppConstant.QuestionTypes.RadioButtonRegister:
                        RadioButtonList rbl = new RadioButtonList { ID = "ctlOptions", CssClass = "survey-rbl" };
                        foreach (var opt in options) rbl.Items.Add(new ListItem(opt.OptionText, opt.OptionID.ToString()));
                        phQuestionArea.Controls.Add(rbl);
                        break;
                    case string t when t.StartsWith(AppConstant.QuestionTypes.CheckBox):
                        CheckBoxList cbl = new CheckBoxList { ID = "ctlOptions", CssClass = "survey-cbl" };
                        foreach (var opt in options) cbl.Items.Add(new ListItem(opt.OptionText, opt.OptionID.ToString()));
                        phQuestionArea.Controls.Add(cbl);
                        break;
                    case AppConstant.QuestionTypes.DropDown:
                        DropDownList ddl = new DropDownList { ID = "ctlOptions", CssClass = "survey-ddl" };
                        ddl.Items.Add(new ListItem("-- Select --", "0"));
                        foreach (var opt in options) ddl.Items.Add(new ListItem(opt.OptionText, opt.OptionID.ToString()));
                        phQuestionArea.Controls.Add(ddl);
                        break;
                    case string t when t.StartsWith(AppConstant.QuestionTypes.TextBoxAlpha.Split('_')[0]):
                        TextBox txt = new TextBox { ID = "ctlOptions", TextMode = TextBoxMode.MultiLine, Rows = 4, CssClass = "form-control" };
                        phQuestionArea.Controls.Add(txt);
                        break;
                }

                btnSkip.Visible = (currentQ.MinSelections == 0 && currentQ.Type != AppConstant.QuestionTypes.RadioButtonRegister);
            }
        }
    }
}