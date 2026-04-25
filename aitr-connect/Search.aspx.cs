using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using aitr_connect.Services;

namespace aitr_connect
{
    public partial class Search : PageBase
    {
        private SearchService _searchService = new SearchService();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!PageValid()) Response.Redirect(AppConstant.PageCatalog.strErrorPage);

            // DDA: Renderar filter baserat på View-kolumnerna
            RenderFilters();

            if (!IsPostBack)
            {
                BindGrid();
            }
        }

        private void BindGrid()
        {
            // Initial laddning: hämtar allt utan filter
            gvUser.DataSource = _searchService.GetFilteredRespondents(this.CurrentConnectionString, null, "");
            gvUser.DataBind();
        }

        private void RenderFilters()
        {
            phFilters.Controls.Clear();
            // Vi hämtar kolumnerna dynamiskt från din View
            var columns = _searchService.GetViewColumns(this.CurrentConnectionString);

            foreach (string colName in columns)
            {
                Panel pnlRow = new Panel { CssClass = "search-row" };

                // Här tog vi bort kolonet efter namnet
                pnlRow.Controls.Add(new Label { Text = colName + " " });

                // Skapar textbox med ID baserat på kolumnnamn
                TextBox tbx = new TextBox
                {
                    ID = "filter_" + colName.Replace(" ", "_"),
                    CssClass = "form-control"
                };
                tbx.Attributes["data-column"] = colName;

                pnlRow.Controls.Add(tbx);
                phFilters.Controls.Add(pnlRow);
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            List<SqlParameter> sqlParams = new List<SqlParameter>();
            StringBuilder whereClause = new StringBuilder();

            // Loopa igenom phFilters för att hitta sökord
            foreach (Control ctrl in phFilters.Controls)
            {
                if (ctrl is Panel pnl)
                {
                    foreach (Control child in pnl.Controls)
                    {
                        if (child is TextBox tbx && !string.IsNullOrWhiteSpace(tbx.Text))
                        {
                            string colName = tbx.Attributes["data-column"];
                            string paramName = "@p" + sqlParams.Count;

                            whereClause.Append($" AND [{colName}] LIKE {paramName}");
                            sqlParams.Add(new SqlParameter(paramName, "%" + tbx.Text + "%"));
                        }
                    }
                }
            }

            gvUser.DataSource = _searchService.GetFilteredRespondents(this.CurrentConnectionString, sqlParams, whereClause.ToString());
            gvUser.DataBind();
        }

        protected void btnBackToDefault_Click(object sender, EventArgs e)
        {
            Response.Redirect(AppConstant.PageCatalog.strDefaultPage);
        }
    }
}