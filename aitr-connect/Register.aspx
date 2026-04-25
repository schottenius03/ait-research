<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="aitr_connect.Register" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Register - AITR Connect</title>
    <link href="Styles/Register_styles.css" rel="stylesheet" type="text/css" />
</head>
<body>
    <form id="form1" runat="server">
        <div class="main-container">
            <div class="nav-header">
                <asp:Button ID="btnBackToDefault" runat="server" Text="← Go back" CssClass="btn btn-back" OnClick="btnBackToDefault_Click" />
            </div>

            <asp:Label ID="lblTitle" runat="server" CssClass="header-title" Text="Register"></asp:Label>
            
            <div class="question-content">
                <asp:PlaceHolder ID="phQuestionArea" runat="server"></asp:PlaceHolder>
            </div>

            <asp:Label ID="lblErrorMessage" runat="server" CssClass="error-label" Visible="false"></asp:Label>

            <div class="action-area">
                <asp:Button ID="btnNext" runat="server" Text="Next" CssClass="btn btn-primary" OnClick="btnNext_Click" />
                <asp:Button ID="btnRegister" runat="server" Text="Register now" CssClass="btn btn-primary" OnClick="btnRegister_Click" Visible="false" />
            </div>
        </div>
    </form>
</body>
</html>