<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="aitr_connect.Register" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Register</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:Button ID="btnBackToDefault" runat="server" Text="< Go back" BorderStyle="None" BackColor="White" BorderWidth="0" OnClick="btnBackToDefault_Click" />
            <br />
            <br />
            <asp:Label ID="lblTitle" runat="server" Text="Register" Font-Size="XX-Large" Font-Bold="True" ForeColor="#003399" ></asp:Label>
            <br />
            <br />
            <asp:Label ID="lblFirstName" runat="server" Text="First name: "></asp:Label>
            <asp:TextBox ID="tbxFirstName" runat="server"></asp:TextBox>
            <br />
            <asp:Label ID="lblLastName" runat="server" Text="Last name: "></asp:Label>
            <asp:TextBox ID="tbxLastName" runat="server"></asp:TextBox>
            <br />
            <asp:Label ID="lblDOB" runat="server" Text="Date of birth: "></asp:Label>
            <asp:TextBox ID="tbxDOB" runat="server"></asp:TextBox>
            <br />
            <asp:Label ID="lblPhoneNr" runat="server" Text="Phone number: "></asp:Label>
            <asp:TextBox ID="tbxPhoneNr" runat="server"></asp:TextBox>
            <br />
            <br />
            <asp:Button ID="btnRegister" runat="server" Text="Register now" Enabled="False" />
        </div>
    </form>
</body>
</html>
