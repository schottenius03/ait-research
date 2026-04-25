<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="aitr_connect.Login" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>AITR - Staff Login</title>
    <link href="Styles/StaffLogin_styles.css" rel="stylesheet" type="text/css" />
    <style type="text/css">
        .btn-primary {}
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="main-container">
            <div class="nav-header">
                <asp:Button ID="btnBackToDefault" runat="server" Text="← Go back" CssClass="btn btn-back" OnClick="btnBackToDefault_Click" />
            </div>

            <span class="header-title">Staff Login</span>
            
            <div class="question-content">
                <p>Please enter your credentials to access the search portal.</p>
                
                <asp:Label ID="lblError" runat="server" CssClass="error-label" Visible="false"></asp:Label>

                <div style="margin-top: 15px;">
                    <label style="font-weight: bold; display: block; margin-bottom: 5px;">Username</label>
                    <asp:TextBox ID="txtUsername" runat="server" CssClass="form-control" placeholder="Enter username"></asp:TextBox>
                </div>

                <div style="margin-top: 15px; margin-bottom: 20px;">
                    <label style="font-weight: bold; display: block; margin-bottom: 5px;">Password</label>
                    <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control" TextMode="Password" placeholder="Enter password"></asp:TextBox>
                </div>
                
                <asp:Button ID="btnLogin" runat="server" Text="Login" CssClass="btn btn-primary" OnClick="btnLogin_Click" />
            </div>
        </div>
    </form>
</body>
</html>