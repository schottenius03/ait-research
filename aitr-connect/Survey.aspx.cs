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
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {

                for (int i = 1; i <= 5; i++)
                {
                    ListItem listItem = new ListItem();
                    listItem.Text = "Option " + i.ToString();

                    rblInput.Items.Add(listItem);
                    listItem.Value = "" + i;

                }
            }
        }

        protected void btnBackToDefault_Click(object sender, EventArgs e)
        {
            Response.Redirect(AppConstant.PageCatalog.strDefaultPage);
        }

        protected void btnNextQuestion_Click(object sender, EventArgs e)
        {
            // radio buttons
            if (rblInput.SelectedItem != null)
            {
                Response.Write("<b>Selected item from RadioButtonLIst control:</b><br/>");
                Response.Write("<ul>");
                Response.Write("<li>" + rblInput.SelectedItem.Text + "</li>");
                Response.Write("</ul>");
            }
        }
    }
}