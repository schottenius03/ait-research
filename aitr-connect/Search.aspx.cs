using System;
using System.Data;
using System.Web.UI.WebControls;
using aitr_connect.Services;

namespace aitr_connect
{
    // ÄNDRA HÄR: Ärv från PageBase istället för Page
    public partial class Search : PageBase
    {
        private SearchService _searchService = new SearchService();

        protected void Page_Load(object sender, EventArgs e)
        {
            // Kontrollera om sidan är giltig (samma mönster som Register)
            if (!PageValid()) Response.Redirect(AppConstant.PageCatalog.strErrorPage);

            // Använd this.CurrentConnectionString som PageBase tillhandahåller
            RenderFilters();

            if (!IsPostBack)
            {
                BindGrid();
            }
        }

        private void BindGrid()
        {
            // Vi skickar med connectionsträngen från PageBase
            gvUser.DataSource = _searchService.GetAllRespondents(this.CurrentConnectionString);
            gvUser.DataBind();
        }

        private void RenderFilters()
        {
            phFilters.Controls.Clear();
            DataTable dt = _searchService.GetFilterCriteria(this.CurrentConnectionString);

            foreach (DataRow row in dt.Rows)
            {
                Panel pnlRow = new Panel { CssClass = "search-row" };
                pnlRow.Controls.Add(new Label { Text = row["questionText"].ToString() + ": " });

                TextBox tbx = new TextBox { ID = "filter_" + row["questionID"] };
                pnlRow.Controls.Add(tbx);

                phFilters.Controls.Add(pnlRow);
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            BindGrid();
        }

        protected void btnBackToDefault_Click(object sender, EventArgs e)
        {
            Response.Redirect(AppConstant.PageCatalog.strDefaultPage);
        }
    }
}