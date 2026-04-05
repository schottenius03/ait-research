<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Search.aspx.cs" Inherits="aitr_connect.Search" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Search</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:Button ID="btnBackToDefault" runat="server" Text="< Go back" BorderStyle="None" BackColor="White" BorderWidth="0" OnClick="btnBackToDefault_Click" />
            <br />
            <br />
            <asp:Label ID="lblTitle" runat="server" Text="Search" Font-Size="XX-Large" Font-Bold="True" ForeColor="#003399" ></asp:Label>
            <br />
            <br />
            <asp:TextBox ID="tbxSearch" runat="server"></asp:TextBox>
            <asp:Button ID="btnSearch" runat="server" Text="Search" Enabled="False" />
            <br />
            <br />
            <asp:GridView ID="gvUser" runat="server"></asp:GridView>
        </div>
    </form>
</body>
</html>
