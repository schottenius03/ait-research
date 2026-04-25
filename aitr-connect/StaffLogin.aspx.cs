using System;
using aitr_connect.Services;

namespace aitr_connect
{
    public partial class Login : PageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Clear error message on initial page load
            if (!IsPostBack)
            {
                lblError.Visible = false;
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            // Get credentials and trim whitespace
            string user = txtUsername.Text.Trim();
            string pass = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                lblError.Text = "Please enter both username and password.";
                lblError.Visible = true;
                return;
            }

            try
            {
                // Instantiate service to validate credentials
                StaffLoginService loginService = new StaffLoginService();

                // Connect to DB and check if staff exists
                bool isValid = loginService.ValidateStaff(this.CurrentConnectionString, user, pass);

                if (isValid)
                {
                    Response.Redirect(AppConstant.PageCatalog.strSearchPage);
                }
                else
                {
                    lblError.Text = "Wrong username or password, try again.";
                    lblError.Visible = true;
                }
            }
            catch (Exception ex)
            {
                lblError.Text = "Database connection failed. Please contact admin.";
                lblError.Visible = true;

                // Debug info visible in VS
                System.Diagnostics.Debug.WriteLine("DB Error: " + ex.Message);
            }
        }

        protected void btnBackToDefault_Click(object sender, EventArgs e)
        {
            // Redirect back to the survey start page using PageCatalog
            Response.Redirect(AppConstant.PageCatalog.strDefaultPage);
        }
    }
}