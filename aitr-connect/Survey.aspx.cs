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
            // Vi validerar fortfarande (ifall de skrivit felaktig text i en TextBox innan de klickade skip)
            if (ValidateSelections())
            {
                MoveToNextQuestion(ignoreRules: true);
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
                    case "RadioButton":
                    case "RadioButton_Register":
                        var rbl = ctl as RadioButtonList;
                        if (rbl != null && !string.IsNullOrEmpty(rbl.SelectedValue))
                            surveyService.SaveAnswer(this.CurrentConnectionString, sessionID, questionID, Convert.ToInt32(rbl.SelectedValue), null);
                        break;
                    case "DropDown":
                        var ddl = ctl as DropDownList;
                        if (ddl != null && ddl.SelectedValue != "0")
                            surveyService.SaveAnswer(this.CurrentConnectionString, sessionID, questionID, Convert.ToInt32(ddl.SelectedValue), null);
                        break;
                    case string t when t.StartsWith("CheckBox"):
                        var cbl = ctl as CheckBoxList;
                        if (cbl != null)
                        {
                            foreach (ListItem item in cbl.Items)
                                if (item.Selected)
                                    surveyService.SaveAnswer(this.CurrentConnectionString, sessionID, questionID, Convert.ToInt32(item.Value), null);
                        }
                        break;
                    case string t when t.StartsWith("TextBox"):
                        var txt = ctl as TextBox;
                        if (txt != null && !string.IsNullOrWhiteSpace(txt.Text))
                        {
                            surveyService.SaveAnswer(this.CurrentConnectionString, sessionID, questionID, null, txt.Text);
                        }
                        break;
                    default:
                        throw new NotSupportedException($"Question type '{type}' is not supported by the system yet.");
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
        private void MoveToNextQuestion(bool ignoreRules = false)
        {
            try
            {
                int currentOrder = Convert.ToInt32(Session[AppConstant.SessionNameList.strQuestionIndex]);

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
                                int? order = surveyService.GetSubQuestionOrder(this.CurrentConnectionString, Convert.ToInt32(item.Value), 1);
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
                        int? order = surveyService.GetSubQuestionOrder(this.CurrentConnectionString, Convert.ToInt32(list.SelectedValue), 1);
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
                        int nextOrder = surveyService.GetNextMainQuestionOrder(this.CurrentConnectionString, currentOrder, 1);
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
        /// Find any rules for the question to trigger a sub question
        /// </summary>
        /// <param name="ctl"></param>
        /// <returns></returns>
        private int? GetTriggeredOrder(Control ctl)
        {
            if (ctl is CheckBoxList cbl)
            {
                foreach (ListItem item in cbl.Items)
                {
                    if (item.Selected)
                    {
                        int? order = surveyService.GetSubQuestionOrder(this.CurrentConnectionString, Convert.ToInt32(item.Value), 1);
                        if (order.HasValue) return order;
                    }
                }
            }
            else if (ctl is ListControl list && !string.IsNullOrEmpty(list.SelectedValue))
            {
                return surveyService.GetSubQuestionOrder(this.CurrentConnectionString, Convert.ToInt32(list.SelectedValue), 1);
            }
            return null;
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
                    case "RadioButton_Register": 
                        RadioButtonList rbl = new RadioButtonList { ID = "ctlOptions", CssClass = "survey-rbl" };
                        foreach (var opt in options) rbl.Items.Add(new ListItem(opt.OptionText, opt.OptionID.ToString()));
                        phQuestionArea.Controls.Add(rbl);
                        break;

                    case string t when t.StartsWith("CheckBox"):
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

                // hide skip button on register question
                if (currentQ.Type == "RadioButton_Register")
                {
                    btnSkip.Visible = false;
                }
                else
                {
                    // only show skip button when selection is 0
                    btnSkip.Visible = (currentQ.MinSelections == 0);
                }
            }
        }

        /// <summary>
        /// redirect to next question 
        /// </summary>
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
        private void HandleNavigationError(Exception ex)
        {
            // If the error is just the redirect itself, do nothing
            if (ex is System.Threading.ThreadAbortException) return;

            Session[AppConstant.SessionNameList.strErroMessage] = "Navigation error: " + ex.Message;
            Response.Redirect(AppConstant.PageCatalog.strErrorPage, false); // false prevent crash
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}