<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ErrorPage.aspx.cs" Inherits="aitr_connect.ErrorPageaspx" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:Label ID="lblTitle" runat="server" Font-Size="XX-Large" Font-Bold="True" ForeColor="Maroon" Text="Something is not right here..."></asp:Label>
            <br />
            <br />
            <asp:Label ID="lblErrorMessage" runat="server" Text=""></asp:Label>
            <br />
            <br />
            <asp:Button ID="btnBackToDefault" runat="server" Text="Go back to start page" OnClick="btnBackToDefault_Click" />
        </div>
    </form>
</body>
</html>
