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
            // check session
            if (Session[AppConstant.SessionNameList.strIsSurveyActive] == null || Session[AppConstant.SessionNameList.strIsSurveyActive].ToString() == "")
            {
                Response.Redirect(AppConstant.PageCatalog.strDefaultPage);
                return;
            }

            // check enviroment 
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
                lblQuestion.Text = "Loading question...";

                RadioButtonList rblInput = new RadioButtonList();
                rblInput.ID = "rblInput";

                if (rblInput.Items.Count == 0)
                {
                    for (int i = 1; i <= 3; i++)
                    {
                        ListItem listItem = new ListItem();
                        listItem.Text = "Option " + i.ToString();

                        rblInput.Items.Add(listItem);
                        listItem.Value = "" + i;

                    }
                }

                // Validation for radioButtonList
                RequiredFieldValidator rfvOption = new RequiredFieldValidator();
                rfvOption.ID = "rfvOption";
                rfvOption.ControlToValidate = rblInput.ID; // connect validation with radioButtonList
                rfvOption.ErrorMessage = "An option is required";
                rfvOption.Display = ValidatorDisplay.None;

                // add to placeholder
                phQuestionArea.Controls.Add(issueList);
                phQuestionArea.Controls.Add(ParseControl(@"<br />"));
                phQuestionArea.Controls.Add(lblQuestion);
                phQuestionArea.Controls.Add(ParseControl(@"<br />"));
                phQuestionArea.Controls.Add(rblInput);
                phQuestionArea.Controls.Add(rfvOption);
            }
            catch (Exception ex)
            {
                // set errorMessage
                Session[AppConstant.SessionNameList.strErroMessage] = "An unexpected error occurred while loading data. Please try again later.";

                // redirect to ErrorPage
                Response.Redirect(AppConstant.PageCatalog.strErrorPage);
            }
        }

        protected void btnBackToDefault_Click(object sender, EventArgs e)
        {
            // Delete value of session 
            Session.Remove(AppConstant.SessionNameList.strIsSurveyActive);
            // redirect to default 
            Response.Redirect(AppConstant.PageCatalog.strDefaultPage);
        }

        protected void btnNextQuestion_Click(object sender, EventArgs e)
        {
            // check validation
            if (Page.IsValid)
            {
                // Get RadioButtonList
                RadioButtonList myRbl = (RadioButtonList)phQuestionArea.FindControl("rblInput");

                if (myRbl != null && myRbl.SelectedItem != null)
                {
                    // get chosen text and value 
                    string selectedText = myRbl.SelectedItem.Text;
                    string selectedValue = myRbl.SelectedValue;

                    Response.Write("You chose " + selectedValue);
                }
            }
        }
    }
}