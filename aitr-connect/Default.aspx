<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="aitr_connect.Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:Label ID="lblTitle" runat="server" Text="Welcome to AIT Research" Font-Size="XX-Large" Font-Bold="True" ForeColor="#003399" ></asp:Label>
            <br />
            <br />
            <asp:Button ID="btnSearch" runat="server" Text="Search repsondents" OnClick="btnSearch_Click" />
            <br />
            <br />
            <asp:Button ID="btnRegister" runat="server" Text="Register respondent" OnClick="btnRegister_Click" />
            <br />
            <br />
            <asp:Button ID="btnSurvey" runat="server" Text="Survey" />
        </div>
    </form>
</body>
</html>
