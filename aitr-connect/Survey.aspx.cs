using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace aitr_connect
{
    public partial class Survey : PageBase
    {
        protected void Page_Init(object sender, EventArgs e)
        {
            // check if survey is active from session var
            if (Session[AppConstant.SessionNameList.strIsSurveyActive] == null)
            {
                Response.Redirect(AppConstant.PageCatalog.strDefaultPage);
                return;
            }

            if (!PageValid())
            {
                Response.Redirect(AppConstant.PageCatalog.strErrorPage);
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            SqlConnection myconn = new SqlConnection(this.CurrentConnectionString);

            try
            {
                myconn.Open();

                // only new respondent if open first time

                if (!IsPostBack && Session["CurrentSessionID"] == null)
                {
                    string sqlRespondent = "INSERT INTO Respondent DEFAULT VALUES; SELECT SCOPE_IDENTITY();";
                    SqlCommand cmdResp = new SqlCommand(sqlRespondent, myconn);
                    int newRespondentID = Convert.ToInt32(cmdResp.ExecuteScalar());

                    // get user IPAdress
                    string userIP = Request.UserHostAddress;
                    string sqlSession = "INSERT INTO ResearchSession (respondentID, sessionDateTime, ipAddress) VALUES (@rID, GETDATE(), @ip); SELECT SCOPE_IDENTITY();";
                    SqlCommand cmdSess = new SqlCommand(sqlSession, myconn);

                    // connect parameters 
                    cmdSess.Parameters.AddWithValue("@rID", newRespondentID);
                    cmdSess.Parameters.AddWithValue("@ip", userIP);

                    int newSessionID = Convert.ToInt32(cmdSess.ExecuteScalar());

                    Session["CurrentRespondentID"] = newRespondentID;
                    Session["CurrentSessionID"] = newSessionID;
                    // set current order to 1
                    if (Session["CurrentOrder"] == null) Session["CurrentOrder"] = 1;
                }

                // get current order
                if (Session["CurrentOrder"] == null)
                {
                    Response.Redirect(AppConstant.PageCatalog.strDefaultPage);
                    return;
                }

                int currentOrder = Convert.ToInt32(Session["CurrentOrder"]);

                // no validation needed for back button
                btnBackToDefault.CausesValidation = false;

                // summary error list 
                ValidationSummary issueList = new ValidationSummary();
                issueList.ID = "issueList";
                issueList.HeaderText = "<b>Please review the following issues:</b>";
                issueList.DisplayMode = ValidationSummaryDisplayMode.List;

                // using classic validation
                this.UnobtrusiveValidationMode = System.Web.UI.UnobtrusiveValidationMode.None;

                // dynamic objects 
                Label lblQuestion = new Label();
                lblQuestion.ID = "lblQuestion";

                // join to get question with text linked to display 
                string sqlGetQuestion = @"SELECT q.questionID, q.questionText, q.questionType 
                          FROM Question q 
                          JOIN SurveyQuestion sq ON q.questionID = sq.questionID 
                          WHERE sq.displayOrder = @order AND sq.surveyID = 1 AND sq.isActive = 1";

                SqlCommand cmdGetQ = new SqlCommand(sqlGetQuestion, myconn);
                cmdGetQ.Parameters.AddWithValue("@order", currentOrder);

                SqlDataReader reader = cmdGetQ.ExecuteReader();

                if (reader.Read())
                {
                    // store question data in session for handling
                    int questionID = Convert.ToInt32(reader["questionID"]);
                    string questionType = reader["questionType"].ToString();

                    Session["CurrentQuestionID"] = questionID;
                    Session["CurrentQuestionType"] = questionType;

                    // set question on label from DB
                    lblQuestion.Text = reader["questionText"].ToString();

                    phQuestionArea.Controls.Add(lblQuestion);
                    phQuestionArea.Controls.Add(new LiteralControl("<br /><br />"));

                    // close reader before fetching options
                    reader.Close();

                    // determine type of listBox
                    // determine type of input and add validation
                    switch (questionType)
                    {
                        case "RadioButton":
                            // Create RadioButtonList
                            RadioButtonList rbl = new RadioButtonList();
                            rbl.ID = "ctrlInput";
                            rbl.CssClass = "form-control-list";
                            LoadListOptions(questionID, rbl, myconn);
                            phQuestionArea.Controls.Add(rbl);

                            // Add Validation for RadioButton
                            RequiredFieldValidator rfvRbl = new RequiredFieldValidator();
                            rfvRbl.ControlToValidate = rbl.ID;
                            rfvRbl.ErrorMessage = "Please select an option.";
                            rfvRbl.ForeColor = System.Drawing.Color.Red;
                            rfvRbl.Display = ValidatorDisplay.Dynamic;
                            phQuestionArea.Controls.Add(rfvRbl);
                            break;

                        case "DropDown":
                            // Create DropDownList
                            DropDownList ddl = new DropDownList();
                            ddl.ID = "ctrlInput";
                            ddl.CssClass = "form-control";
                            ddl.Items.Add(new ListItem("-- Select --", "")); // Empty value for validator to catch
                            LoadListOptions(questionID, ddl, myconn);
                            phQuestionArea.Controls.Add(ddl);

                            // Add Validation for DropDown
                            RequiredFieldValidator rfvDdl = new RequiredFieldValidator();
                            rfvDdl.ControlToValidate = ddl.ID;
                            rfvDdl.InitialValue = ""; // The validator fails if the value is still ""
                            rfvDdl.ErrorMessage = "Please choose an item from the list.";
                            rfvDdl.ForeColor = System.Drawing.Color.Red;
                            rfvDdl.Display = ValidatorDisplay.Dynamic;
                            phQuestionArea.Controls.Add(rfvDdl);
                            break;

                        case "CheckBox":
                            // Create CheckBoxList
                            CheckBoxList cbl = new CheckBoxList();
                            cbl.ID = "ctrlInput";
                            LoadListOptions(questionID, cbl, myconn);
                            phQuestionArea.Controls.Add(cbl);

                            // custom validation
                            CustomValidator cvCbl = new CustomValidator();
                            cvCbl.ID = "cvCbl";
                            // inline event handler to validate on server number of ticked boxes 
                            cvCbl.ServerValidate += (source, args) => {
                                int selectedCount = 0;
                                foreach (ListItem li in cbl.Items) { if (li.Selected) selectedCount++; }

                                if (questionID == 6)
                                {
                                    // maxLimit 4 Q6
                                    args.IsValid = (selectedCount >= 1 && selectedCount <= 4);
                                    cvCbl.ErrorMessage = "Please select a maximum of 4 options.";
                                }
                                else if (questionID == 8)
                                {
                                    // Second limit: Max 2 Q8
                                    args.IsValid = (selectedCount >= 1 && selectedCount <= 2);
                                    cvCbl.ErrorMessage = "Please select a maximum of 2 options.";
                                }
                                else
                                {
                                    // Default: at least one must be selected
                                    args.IsValid = (selectedCount >= 1);
                                    cvCbl.ErrorMessage = "Please select at least one checkbox.";
                                }
                            };
                            cvCbl.ForeColor = System.Drawing.Color.Red;
                            cvCbl.Display = ValidatorDisplay.Dynamic;
                            phQuestionArea.Controls.Add(cvCbl);
                            break;

                        case "TextBox":
                            // Create TextBox
                            TextBox txt = new TextBox();
                            txt.ID = "ctrlInput";
                            txt.CssClass = "form-control";
                            txt.TextMode = TextBoxMode.SingleLine;
                            phQuestionArea.Controls.Add(txt);

                            // Add Validation for TextBox
                            RequiredFieldValidator rfvTxt = new RequiredFieldValidator();
                            rfvTxt.ControlToValidate = txt.ID;
                            rfvTxt.ErrorMessage = "This field is required.";
                            rfvTxt.ForeColor = System.Drawing.Color.Red;
                            rfvTxt.Display = ValidatorDisplay.Dynamic;
                            phQuestionArea.Controls.Add(rfvTxt);
                            break;
                    }
                }
                else
                {
                    // No more questions
                    reader.Close();

                    // Remove values for clean next ResearchSession
                    Session.Remove(AppConstant.SessionNameList.strIsSurveyActive);
                    Session.Remove("CurrentRespondentID");
                    Session.Remove("CurrentSessionID");
                    Session.Remove("CurrentOrder");
                    Session.Remove("CurrentQuestionID");
                    Session.Remove("CurrentQuestionType");

                    // Redirect to default page
                    Response.Redirect(AppConstant.PageCatalog.strDefaultPage);
                }
            }
            catch (Exception ex)
            {
                // set errorMessage
                Session[AppConstant.SessionNameList.strErroMessage] = "An unexpected error occurred while loading data. Please try again later.";

                // redirect to ErrorPage
                Response.Redirect(AppConstant.PageCatalog.strErrorPage);
            }
        }

        protected void btnNextQuestion_Click(object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                SqlConnection myconn = new SqlConnection(this.CurrentConnectionString);

                try
                {
                    myconn.Open();

                    // current question from session
                    int sessionID = Convert.ToInt32(Session["CurrentSessionID"]);
                    int questionID = Convert.ToInt32(Session["CurrentQuestionID"]);
                    string questionType = Session["CurrentQuestionType"].ToString();

                    // get input control
                    Control myControl = phQuestionArea.FindControl("ctrlInput");

                    // determine datatype and set options and save choice by respondent
                    if (questionType == "RadioButton" || questionType == "DropDown")
                    {
                        ListControl list = (ListControl)myControl;
                        if (list.SelectedItem != null)
                        {
                            SaveAnswer(sessionID, questionID, list.SelectedValue, null, myconn);
                        }
                    }
                    else if (questionType == "TextBox")
                    {
                        TextBox txt = (TextBox)myControl;
                        string answerText = txt.Text;

                        // save to respondent
                        SaveAnswer(sessionID, questionID, null, answerText, myconn);

                        // get label using placeholder
                        Label myLabel = (Label)phQuestionArea.FindControl("lblQuestion");

                        if (myLabel != null) // matching
                        {
                            string qText = myLabel.Text.ToLower();
                            if (qText.Contains("email") || qText.Contains("e-post"))
                            {
                                int respondentID = Convert.ToInt32(Session["CurrentRespondentID"]);
                                UpdateRespondentEmail(respondentID, answerText, myconn);
                            }
                        }
                    }
                    else if (questionType == "CheckBox")
                    {
                        CheckBoxList cbl = (CheckBoxList)myControl;
                        // loop through all 
                        foreach (ListItem item in cbl.Items)
                        {
                            if (item.Selected)
                            {
                                SaveAnswer(sessionID, questionID, item.Value, null, myconn);
                            }
                        }
                    }

                    // next question
                    int nextOrder = Convert.ToInt32(Session["CurrentOrder"]) + 1;
                    Session["CurrentOrder"] = nextOrder;

                    // refresh its own page
                    Response.Redirect(Request.RawUrl, false); // prevent ThreadAbortion
                    Context.ApplicationInstance.CompleteRequest(); // Avslutar begäran snyggt
                }
                catch (Exception ex)
                {
                    Session[AppConstant.SessionNameList.strErroMessage] = "Error saving answer: " + ex.Message;
                    Response.Redirect(AppConstant.PageCatalog.strErrorPage);
                }
                finally
                {
                    // always close connection once survey is completed 
                    if (myconn.State == System.Data.ConnectionState.Open) { myconn.Close(); }
                }
            }
        }

        // custom method for inserting data to DB
        private void SaveAnswer(int sID, int qID, string oID, string txt, SqlConnection conn)
        {
            string sqlInsert = "INSERT INTO ResponseAnswer (sessionID, questionID, optionID, textAnswer, dateRecorded) VALUES (@sID, @qID, @oID, @txt, GETDATE())";
            SqlCommand cmd = new SqlCommand(sqlInsert, conn);

            cmd.Parameters.AddWithValue("@sID", sID);
            cmd.Parameters.AddWithValue("@qID", qID);

            // NUll handling for DB
            if (string.IsNullOrEmpty(oID)) cmd.Parameters.AddWithValue("@oID", DBNull.Value);
            else cmd.Parameters.AddWithValue("@oID", oID);

            if (string.IsNullOrEmpty(txt)) cmd.Parameters.AddWithValue("@txt", DBNull.Value);
            else cmd.Parameters.AddWithValue("@txt", txt);

            cmd.ExecuteNonQuery();
        }

        // get all options for active question
        private void LoadListOptions(int qID, ListControl control, SqlConnection conn)
        {
            // query to get options for question
            string sqlOptions = "SELECT optionID, optionText FROM [Option] WHERE questionID = @qID AND isActive = 1";
            SqlCommand cmdOptions = new SqlCommand(sqlOptions, conn);
            cmdOptions.Parameters.AddWithValue("@qID", qID);

            using (SqlDataReader optReader = cmdOptions.ExecuteReader())
            {
                while (optReader.Read())
                {
                    ListItem item = new ListItem();
                    item.Text = optReader["optionText"].ToString();
                    item.Value = optReader["optionID"].ToString(); // save to ResponseAnswer
                    control.Items.Add(item);
                }
            }
        }

        // update email address for respondent from cration of profile (still anonoums
        private void UpdateRespondentEmail(int rID, string email, SqlConnection conn)
        {
            string sqlUpdate = "UPDATE Respondent SET email = @email WHERE respondentID = @rID";

            // command
            SqlCommand cmd = new SqlCommand(sqlUpdate, conn);

            // prevent SQL Injection using parameters
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@rID", rID);

            // execute 
            cmd.ExecuteNonQuery();
        }
        protected void btnBackToDefault_Click(object sender, EventArgs e)
        {
            // Delete value of session 
            Session.Remove(AppConstant.SessionNameList.strIsSurveyActive);
            // redirect to default 
            Response.Redirect(AppConstant.PageCatalog.strDefaultPage);
        }
    }
}