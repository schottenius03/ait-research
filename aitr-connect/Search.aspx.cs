using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using aitr_connect.Services;

namespace aitr_connect
{
    public partial class Search : PageBase
    {
        private SearchService _searchService = new SearchService();
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            try
            {
                RenderFilters();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error in OnInit (RenderFilters): " + ex.Message);
                Response.Redirect(AppConstant.PageCatalog.strErrorPage);
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Validera sidan via PageBase
            if (!PageValid()) Response.Redirect(AppConstant.PageCatalog.strErrorPage);

            // RenderFilters anropas via OnInit för att hantera ViewState korrekt

            if (!IsPostBack)
            {
                BindGrid();
            }
        }

        /// <summary>
        /// fetches and binds the initial data to the GridView
        /// </summary>
        private void BindGrid()
        {
            try
            {
                gvUser.DataSource = _searchService.GetFilteredRespondents(this.CurrentConnectionString, null, "");
                gvUser.DataBind();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error binding grid: " + ex.Message);
                Response.Redirect(AppConstant.PageCatalog.strErrorPage);
            }
        }

        /// <summary>
        /// Generate filter controls for radioButtons and checkBoxes
        /// </summary>
        private void RenderFilters()
        {
            phFilters.Controls.Clear();
            var columns = _searchService.GetViewColumns(this.CurrentConnectionString);

            foreach (string colName in columns)
            {
                // Skip ID columns
                if (colName.ToLower().EndsWith("id")) continue;

                Panel pnlRow = new Panel { CssClass = "filter-group", Style = { ["margin-bottom"] = "15px" } };

                // header
                pnlRow.Controls.Add(new Label
                {
                    Text = colName,
                    CssClass = "form-label",
                    Style = { ["display"] = "block", ["font-weight"] = "bold" }
                });

                var metadata = _searchService.GetColumnMetadata(this.CurrentConnectionString, colName);

                if (metadata.Options.Count > 0)
                {
                    // DropDown for categories
                    DropDownList ddl = new DropDownList
                    {
                        ID = "filter_" + colName.Replace(" ", "_"),
                        CssClass = "form-control"
                    };
                    ddl.Attributes["data-column"] = colName;
                    ddl.Items.Add(new ListItem("-- Select All --", ""));

                    foreach (var opt in metadata.Options)
                    {
                        // using text for text and value to match view
                        ddl.Items.Add(new ListItem(opt.Text, opt.Value));
                    }
                    pnlRow.Controls.Add(ddl);
                }
                else
                {
                    // textbox for free search
                    TextBox tbx = new TextBox
                    {
                        ID = "filter_" + colName.Replace(" ", "_"),
                        CssClass = "form-control"
                    };
                    tbx.Attributes["data-column"] = colName;
                    pnlRow.Controls.Add(tbx);
                }

                phFilters.Controls.Add(pnlRow);
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                List<SqlParameter> sqlParams = new List<SqlParameter>();
                StringBuilder whereClause = new StringBuilder();

                foreach (Control pnl in phFilters.Controls)
                {
                    if (pnl is Panel row)
                    {
                        foreach (Control child in row.Controls)
                        {
                            string val = "";
                            string colName = "";
                            bool isDropDown = false;

                            if (child is DropDownList ddl && !string.IsNullOrEmpty(ddl.SelectedValue))
                            {
                                val = ddl.SelectedValue;
                                colName = ddl.Attributes["data-column"];
                                isDropDown = true;
                            }
                            else if (child is TextBox tbx && !string.IsNullOrWhiteSpace(tbx.Text))
                            {
                                val = tbx.Text.Trim();
                                colName = tbx.Attributes["data-column"];
                                isDropDown = false;
                            }

                            if (!string.IsNullOrEmpty(val))
                            {
                                string paramName = "@p" + sqlParams.Count;

                                // checkBox categories
                                if (isDropDown)
                                {
                                    if (colName == "Sports" || colName == "Travel Destinations" ||
                                        colName == "Bank" || colName == "Bank Services" ||
                                        colName == "Newspaper" || colName == "News Section")
                                    {
                                        // include more than one option from filtering
                                        whereClause.Append($" AND [{colName}] LIKE {paramName}");
                                        sqlParams.Add(new SqlParameter(paramName, "%" + val + "%"));
                                    }
                                    else
                                    {
                                        // rest of dropdowns
                                        whereClause.Append($" AND [{colName}] = {paramName}");
                                        sqlParams.Add(new SqlParameter(paramName, val));
                                    }
                                }
                                else
                                {
                                    // including for textboxes
                                    whereClause.Append($" AND [{colName}] LIKE {paramName}");
                                    sqlParams.Add(new SqlParameter(paramName, "%" + val + "%"));
                                }
                            }
                        }
                    }
                }

                gvUser.DataSource = _searchService.GetFilteredRespondents(this.CurrentConnectionString, sqlParams, whereClause.ToString());
                gvUser.DataBind();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error in btnSearch_Click: " + ex.Message);
                Response.Redirect(AppConstant.PageCatalog.strErrorPage);
            }
        }

        protected void btnSignOut_Click(object sender, EventArgs e)
        {
            Response.Redirect(AppConstant.PageCatalog.strDefaultPage);
        }
    }
}