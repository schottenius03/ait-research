<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="aitr_connect.Register" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Register</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:Button ID="btnBackToDefault" runat="server" Text="Go back" OnClick="btnBackToDefault_Click" />
            <br />
            <br />
            
            <asp:Label ID="lblTitle" runat="server" Text="Register" Font-Size="XX-Large" Font-Bold="True" ForeColor="#003399" ></asp:Label>
            <br />
            <br />

            <asp:PlaceHolder ID="phQuestionArea" runat="server"></asp:PlaceHolder>
            <br />
            
            <asp:Label ID="lblErrorMessage" runat="server" ForeColor="Red" Font-Bold="true"></asp:Label>
            <br />
            <br />

            <asp:Button ID="btnNext" runat="server" Text="Next" OnClick="btnNext_Click" />
            <asp:Button ID="btnRegister" runat="server" Text="Register now" OnClick="btnRegister_Click" Visible="false" />
        </div>
    </form>
</body>
</html>