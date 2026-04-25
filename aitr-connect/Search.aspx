<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Search.aspx.cs" Inherits="aitr_connect.Search" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Search - AIT Research</title>
    <link href="Styles/Search_styles.css" rel="stylesheet" type="text/css" />
</head>
<body>
    <form id="form1" runat="server">
        <div class="container">
            <div class="header-row">
                <asp:Button ID="btnBackToDefault" runat="server" Text="← Go Back" OnClick="btnBackToDefault_Click" CssClass="btn-back" />
                <h1 class="search-title">Search</h1>
                <div style="width: 100px;"></div> </div>
            
            <hr />

            <div class="filter-section">
                <asp:PlaceHolder ID="phFilters" runat="server"></asp:PlaceHolder>
            </div>

            <div class="button-section">
                <asp:Button ID="btnSearch" runat="server" Text="Search Respondents" OnClick="btnSearch_Click" CssClass="btn-main" />
            </div>

            <hr />

            <div class="grid-container">
                <asp:GridView ID="gvUser" runat="server" 
                    Width="100%" 
                    CssClass="results-grid" 
                    AutoGenerateColumns="true" 
                    EmptyDataText="No respondents match your current filters." 
                    GridLines="None">
                </asp:GridView>
            </div>
        </div>
    </form>
</body>
</html>