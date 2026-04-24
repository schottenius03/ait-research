using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
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
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // verify enviroment 
            if (!PageValid()) return;

            try
            {
                // create new respondent and session 
                if (!IsPostBack && Session[AppConstant.SessionNameList.strSessionID] == null)
                {
                    var result = surveyService.CreateNewSurveySession(this.CurrentConnectionString, Request.UserHostAddress);

                    // store in sessions
                    Session[AppConstant.SessionNameList.strRespondentID] = result.RespondentID;
                    Session[AppConstant.SessionNameList.strSessionID] = result.SessionID;
                }

                // Verify question index
                if (Session[AppConstant.SessionNameList.strQuestionIndex] == null)
                {
                    Response.Redirect(AppConstant.PageCatalog.strDefaultPage);
                    return;
                }

                // get current order from session
                int currentOrder = Convert.ToInt32(Session[AppConstant.SessionNameList.strQuestionIndex]);

                // fetch the question through the service
                aitr_connect.Services.SurveyQuestion currentQ = surveyService.GetQuestionByOrder(this.CurrentConnectionString, currentOrder);

                if (currentQ != null)
                {
                    // store current question info in session for logic later (e.g. in btnNext_Click)
                    Session[AppConstant.SessionNameList.strQuestionID] = currentQ.ID;
                    Session[AppConstant.SessionNameList.strQuestionType] = currentQ.Type;

                    // UI logic: setup labels
                    lblQuestionNumber.Text = currentQ.ID.ToString();

                    // create and add the question text label
                    Label lblQuestion = new Label { ID = "lblQuestion", Text = currentQ.Text };
                    phQuestionArea.Controls.Add(lblQuestion);
                    phQuestionArea.Controls.Add(new LiteralControl("<br /><br />"));

                    // present the question in it's form 
                    var options = surveyService.GetOptionsByQuestionID(this.CurrentConnectionString, currentQ.ID);

                    switch (currentQ.Type)
                    {
                        case "RadioButton":
                            // create list 
                            RadioButtonList rbl = new RadioButtonList { ID = "ctlOptions", CssClass = "survey-rbl" };

                            // loop all alternatives
                            foreach (var opt in options)
                            {
                                rbl.Items.Add(new ListItem(opt.OptionText, opt.OptionID.ToString()));
                            }
                            phQuestionArea.Controls.Add(rbl);
                            break;

                        case "CheckBox":
                            CheckBoxList cbl = new CheckBoxList { ID = "ctlOptions", CssClass = "survey-cbl" };
                            foreach (var opt in options)
                            {
                                cbl.Items.Add(new ListItem(opt.OptionText, opt.OptionID.ToString()));
                            }
                            phQuestionArea.Controls.Add(cbl);
                            break;

                        case "DropDown":
                            DropDownList ddl = new DropDownList { ID = "ctlOptions", CssClass = "survey-ddl" };
                            ddl.Items.Add(new ListItem("-- Select an option --", "0"));
                            foreach (var opt in options)
                            {
                                ddl.Items.Add(new ListItem(opt.OptionText, opt.OptionID.ToString()));
                            }
                            phQuestionArea.Controls.Add(ddl);
                            break;

                        case "TextBox":
                            // create textBox directly
                            TextBox txt = new TextBox { ID = "ctlOptions", TextMode = TextBoxMode.MultiLine, Rows = 4, CssClass = "form-control" };
                            phQuestionArea.Controls.Add(txt);
                            break;
                    }
                }
                else
                {
                    // clean sessions when survey is done
                    Session.Remove(AppConstant.SessionNameList.strIsSurveyActive);
                    Response.Redirect(AppConstant.PageCatalog.strDefaultPage);
                }

            }
            catch (Exception ex)
            {
                Session[AppConstant.SessionNameList.strErroMessage] = "An unexpected error occurred: " + ex.Message;
                Response.Redirect(AppConstant.PageCatalog.strErrorPage);
            }
        }
    }
}















/*



            // save questionIndex as a variable 
            int currentOrder = Convert.ToInt32(Session[AppConstant.SessionNameList.strQuestionIndex]);

                // no validation needed for going back to Default page
                btnBackToDefault.CausesValidation = false;

                // dynamic objects 
                Label lblQuestion = new Label();
                lblQuestion.ID = "lblQuestion";

                // get question from DB by using current order ID and make sure question is active 
                string sqlGetQuestion = @"SELECT q.questionID, q.questionText, q.questionType, q.minSelections, q.maxSelections 
                                          FROM Question q 
                                          JOIN SurveyQuestion sq ON q.questionID = sq.questionID 
                                          WHERE sq.displayOrder = @order AND sq.surveyID = 1 AND sq.isActive = 1";

                SqlCommand cmdGetQ = new SqlCommand(sqlGetQuestion, myconn);
                cmdGetQ.Parameters.AddWithValue("@order", currentOrder);

                // store question to var 
                SurveyQuestion currentQ = null;

                // using will close reader when done
                using (reader = cmdGetQ.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // store query data to variables 
                        currentQ = new SurveyQuestion
                        {
                            ID = Convert.ToInt32(reader["questionID"]),
                            Text = reader["questionText"].ToString(),
                            Type = reader["questionType"].ToString(),
                            MinSelections = Convert.ToInt32(reader["minSelections"]),
                            MaxSelections = Convert.ToInt32(reader["maxSelections"])
                        };
                    }
                }

                if (currentQ != null)
                {
                    // save to sessions
                    Session["CurrentQuestionID"] = currentQ.ID;
                    Session["CurrentQuestionType"] = currentQ.Type;

                    // label title to id number 
                    lblQuestionNumber.Text = currentQ.ID.ToString();

                    // assign questionText from DB to label 
                    lblQuestion.Text = currentQ.Text;

                    // add to placeholder and the website
                    phQuestionArea.Controls.Add(lblQuestion);
                    phQuestionArea.Controls.Add(new LiteralControl("<br /><br />"));

                    switch (currentQ.Type)
                    {
                        case "RadioButton":
                            // Create RadioButtonList
                            RadioButtonList rbl = new RadioButtonList { ID = "ctrlInput", CssClass = "form-control-list" };
                            LoadListOptions(currentQ.ID, rbl, myconn);
                            phQuestionArea.Controls.Add(rbl);

                            // Add Validation for RadioButton
                            RequiredFieldValidator rfvRbl = new RequiredFieldValidator
                            {
                                ControlToValidate = rbl.ID,
                                ErrorMessage = "Please select an option.",
                                ForeColor = System.Drawing.Color.Red,
                                Display = ValidatorDisplay.Dynamic
                            };
                            phQuestionArea.Controls.Add(rfvRbl);
                            break;

                        case "DropDown":
                            // Create DropDownList
                            DropDownList ddl = new DropDownList { ID = "ctrlInput", CssClass = "form-control" };
                            ddl.Items.Add(new ListItem("-- Select --", ""));
                            LoadListOptions(currentQ.ID, ddl, myconn);
                            phQuestionArea.Controls.Add(ddl);

                            // Add Validation for DropDown
                            RequiredFieldValidator rfvDdl = new RequiredFieldValidator
                            {
                                ControlToValidate = ddl.ID,
                                InitialValue = "",
                                ErrorMessage = "Please choose an item from the list",
                                ForeColor = System.Drawing.Color.Red,
                                Display = ValidatorDisplay.Dynamic
                            };
                            phQuestionArea.Controls.Add(rfvDdl);
                            break;

                        case "CheckBox":
                            // Create CheckBoxList
                            CheckBoxList cbl = new CheckBoxList { ID = "ctrlInput", CssClass = "form-control-list" };
                            LoadListOptions(currentQ.ID, cbl, myconn);
                            phQuestionArea.Controls.Add(cbl);

                            // store min/max value for validations
                            int minLimit = currentQ.MinSelections;
                            int maxLimit = currentQ.MaxSelections;

                            // custom validation
                            CustomValidator cvCbl = new CustomValidator { ID = "cvCbl" };
                            cvCbl.ServerValidate += (source, args) =>
                            {
                                int selectedCount = 0;
                                foreach (ListItem li in cbl.Items) { if (li.Selected) selectedCount++; }
                                args.IsValid = (selectedCount >= minLimit && selectedCount <= maxLimit);

                                // error message to fit req from DB
                                if (minLimit > 0)
                                    cvCbl.ErrorMessage = $"Please select up to {maxLimit} options.";
                                else
                                    cvCbl.ErrorMessage = $"You can select a maximum of {maxLimit} options.";
                            };
                            cvCbl.ForeColor = System.Drawing.Color.Red;
                            cvCbl.Display = ValidatorDisplay.Dynamic;
                            phQuestionArea.Controls.Add(cvCbl);
                            break;

                        case "TextBox":
                            // create textBox
                            TextBox txt = new TextBox { ID = "ctrlInput", CssClass = "form-control" };
                            phQuestionArea.Controls.Add(txt);

                            // get question and validation requirment
                            string qTextLower = currentQ.Text.ToLower();

                            // min/max values for optional/req questions
                            if (currentQ.MinSelections > 0)
                            {
                                RequiredFieldValidator rfvTxt = new RequiredFieldValidator
                                {
                                    ControlToValidate = txt.ID,
                                    ErrorMessage = "This field is required.",
                                    ForeColor = System.Drawing.Color.Red,
                                    Display = ValidatorDisplay.Dynamic
                                };
                                phQuestionArea.Controls.Add(rfvTxt);
                            }

                            if (qTextLower.Contains("email"))
                            {
                                // valid email format 
                                RegularExpressionValidator revEmail = new RegularExpressionValidator
                                {
                                    ControlToValidate = txt.ID,
                                    ValidationExpression = @"^[^@\s]+@[^@\s]+\.[a-zA-Z]{2,}$",
                                    ErrorMessage = "Please enter a valid email address.",
                                    ForeColor = System.Drawing.Color.Red,
                                    Display = ValidatorDisplay.Dynamic
                                };
                                phQuestionArea.Controls.Add(revEmail);
                            }
                            else if (qTextLower.Contains("postcode"))
                            {
                                // require 4 digits
                                RegularExpressionValidator revPost = new RegularExpressionValidator
                                {
                                    ControlToValidate = txt.ID,
                                    ValidationExpression = @"^\d{4}$",
                                    ErrorMessage = "Postcode must be 4 digits.",
                                    ForeColor = System.Drawing.Color.Red,
                                    Display = ValidatorDisplay.Dynamic
                                };
                                phQuestionArea.Controls.Add(revPost);
                            }
                            else if (qTextLower.Contains("suburb"))
                            {
                                // require letters only 
                                RegularExpressionValidator revSuburb = new RegularExpressionValidator
                                {
                                    ControlToValidate = txt.ID,
                                    ValidationExpression = @"^[a-zA-Z\s]+$",
                                    ErrorMessage = "Suburb name must contain only letters.",
                                    ForeColor = System.Drawing.Color.Red,
                                    Display = ValidatorDisplay.Dynamic
                                };
                                phQuestionArea.Controls.Add(revSuburb);
                            }
                            break;
                    }
                }
                else
                {
                    // no more question of survey - clear session variables
                    Session.Remove(AppConstant.SessionNameList.strIsSurveyActive);
                    Session.Remove("CurrentRespondentID");
                    Session.Remove("CurrentSessionID");
                    Session.Remove(AppConstant.SessionNameList.strQuestionIndex);

                    // avoid exception by using false for CompleteRequest
                    Response.Redirect(AppConstant.PageCatalog.strDefaultPage, false);
                    Context.ApplicationInstance.CompleteRequest();
                    return;
                }
            }
            catch (Exception ex)
            {
                // set errorMessage
                Session[AppConstant.SessionNameList.strErroMessage] = "An unexpected error occurred: " + ex.Message;

                // redirect to ErrorPage
                Response.Redirect(AppConstant.PageCatalog.strErrorPage);
            }
            finally
            {
                // close connection to DB
                if (reader != null && !reader.IsClosed) reader.Close();
                if (myconn != null && myconn.State != ConnectionState.Closed) myconn.Close();
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
                    int currentOrder = Convert.ToInt32(Session[AppConstant.SessionNameList.strQuestionIndex]);

                    // get input control
                    Control myControl = phQuestionArea.FindControl("ctrlInput");

                    // determine datatype and set options and save choice by respondent
                    if (myControl is ListControl listControl)
                    {
                        // Handles RadioButton, DropDown and CheckBox logic in one loop
                        foreach (ListItem item in listControl.Items)
                        {
                            if (item.Selected)
                            {
                                SaveAnswer(sessionID, questionID, item.Value, null, myconn);
                            }
                        }
                    }
                    else if (myControl is TextBox txt)
                    {
                        string answerText = txt.Text;
                        SaveAnswer(sessionID, questionID, null, answerText, myconn);
                    }

                    int nextOrder = -1;

                    // Simplified logic: Find the next available displayOrder that is active
                    string sqlGetNext = @"SELECT MIN(sq.displayOrder) 
                                 FROM SurveyQuestion sq 
                                 WHERE sq.surveyID = 1 
                                 AND sq.displayOrder > @currentOrder 
                                 AND sq.isActive = 1";

                    using (SqlCommand cmdNext = new SqlCommand(sqlGetNext, myconn))
                    {
                        cmdNext.Parameters.AddWithValue("@currentOrder", currentOrder);
                        object result = cmdNext.ExecuteScalar();

                        if (result != DBNull.Value && result != null)
                        {
                            nextOrder = Convert.ToInt32(result);
                        }
                    }

                    Session[AppConstant.SessionNameList.strQuestionIndex] = nextOrder;

                    // refresh its own page
                    Response.Redirect(Request.RawUrl, false);
                    Context.ApplicationInstance.CompleteRequest();
                }
                catch (Exception ex)
                {
                    // set errorMessage
                    Session[AppConstant.SessionNameList.strErroMessage] = "Error: " + ex.Message;
                    Response.Redirect(AppConstant.PageCatalog.strErrorPage);
                }
                finally
                {
                    if (myconn.State == ConnectionState.Open) { myconn.Close(); }
                }
            }
        }

        /// <summary>
        /// save respondent's answer to ResponseAnswer
        /// handle both altternative questions and txt question 
        /// optional questions are set to null if left empty by user
        /// </summary>
        private void SaveAnswer(int sID, int qID, string oID, string txt, SqlConnection conn)
        {
            string sqlInsert = "INSERT INTO ResponseAnswer (sessionID, questionID, optionID, textAnswer, dateRecorded) VALUES (@sID, @qID, @oID, @txt, GETDATE())";
            SqlCommand cmd = new SqlCommand(sqlInsert, conn);

            cmd.Parameters.AddWithValue("@sID", sID);
            cmd.Parameters.AddWithValue("@qID", qID);

            // handle null values 
            cmd.Parameters.AddWithValue("@oID", (object)oID ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@txt", (object)txt ?? DBNull.Value);

            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// get all options alternatives from DB to the question
        /// display all the options in specified list 
        /// </summary>
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

        protected void btnBackToDefault_Click(object sender, EventArgs e)
        {
            // Delete value of session 
            Session.Remove(AppConstant.SessionNameList.strIsSurveyActive);
            // redirect to default 
            Response.Redirect(AppConstant.PageCatalog.strDefaultPage);
        }

        protected void btnSkip_Click(object sender, EventArgs e)
        {
            int currentOrder = Convert.ToInt32(Session[AppConstant.SessionNameList.strQuestionIndex]);
            int nextOrder = -1;

            using (SqlConnection myconn = new SqlConnection(this.CurrentConnectionString))
            {
                myconn.Open();

                // Vi letar efter den MINSTA displayOrder som är STÖRRE än nuvarande,
                // men vi exkluderar alla frågor som finns med i QuestionRule som 'childQuestionID'.
                // På så sätt hittar vi nästa fråga som inte är en sub-question.
                string sqlGetNextMain = @"
            SELECT MIN(sq.displayOrder) 
            FROM SurveyQuestion sq 
            WHERE sq.surveyID = 1 
            AND sq.displayOrder > @currentOrder 
            AND sq.isActive = 1
            AND sq.questionID NOT IN (SELECT childQuestionID FROM QuestionRule)";

                using (SqlCommand cmdNext = new SqlCommand(sqlGetNextMain, myconn))
                {
                    cmdNext.Parameters.AddWithValue("@currentOrder", currentOrder);
                    object result = cmdNext.ExecuteScalar();

                    if (result != DBNull.Value && result != null)
                    {
                        nextOrder = Convert.ToInt32(result);
                    }
                }
            }

            // Uppdatera index. Om nextOrder är -1 kommer Page_Load hantera att enkäten är slut.
            Session[AppConstant.SessionNameList.strQuestionIndex] = nextOrder;

            Response.Redirect(Request.RawUrl);
        }
    }
} */