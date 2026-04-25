<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Search.aspx.cs" Inherits="aitr_connect.Search" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Staff Search - AITR Research</title>
    <link href="Styles/Search_styles.css" rel="stylesheet" type="text/css" />
</head>
<body>
    <form id="form1" runat="server">
        <div class="container">
            <asp:Button ID="btnBackToDefault" runat="server" Text="← Go Back" OnClick="btnBackToDefault_Click" CssClass="btn" />
            <br /><br />
            <asp:Label ID="lblTitle" runat="server" Text="Staff Search (DDA)" Font-Size="XX-Large" Font-Bold="True" ForeColor="#003399"></asp:Label>
            <hr />

            <div class="filter-section">
                <asp:PlaceHolder ID="phFilters" runat="server"></asp:PlaceHolder>
            </div>

            <div class="button-section">
                <asp:Button ID="btnSearch" runat="server" Text="Search Respondents" OnClick="btnSearch_Click" CssClass="btn" BackendColor="#003399" ForeColor="White" Font-Bold="true" />
            </div>

            <hr />

            <div style="overflow-x: auto;">
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