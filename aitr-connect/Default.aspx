<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="aitr_connect.Default" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Home - AIT Research</title>
    <link href="Styles/Default_styles.css" rel="stylesheet" type="text/css" />
</head>
<body>
    <form id="form1" runat="server">
        <div class="main-container">
            <asp:Label ID="lblTitle" runat="server" CssClass="header-title" Text="AIT Research"></asp:Label>
            <asp:Label ID="lblSub" runat="server" CssClass="sub-title" Text="Select a module to continue"></asp:Label>
            
            <div class="menu-area">
                <asp:Button ID="btnSearch" runat="server" Text="Staff login" CssClass="btn btn-primary" OnClick="btnSearch_Click" />
                
                <asp:Button ID="btnRegister" runat="server" Text="Register Respondent" CssClass="btn btn-primary" OnClick="btnRegister_Click" />
                
                <asp:Button ID="btnSurvey" runat="server" Text="Take Survey" CssClass="btn btn-primary" OnClick="btnSurvey_Click" />
            </div>
        </div>
    </form>
</body>
</html>