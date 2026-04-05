<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Survey.aspx.cs" Inherits="aitr_connect.Survey" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Survey</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:Button ID="btnBackToDefault" runat="server" Text="< Go back" BorderStyle="None" BackColor="White" BorderWidth="0" OnClick="btnBackToDefault_Click" />
            <br />
            <br />
            <asp:Label ID="lblTitle" runat="server" Text="Survey" Font-Size="XX-Large" Font-Bold="True" ForeColor="#003399" ></asp:Label>
            <br />
            <br />
            <asp:Label ID="lblQuestion" runat="server" Text="Loading question..."></asp:Label>
            <br />
            <br />
            <asp:RadioButtonList ID="rblInput" runat="server"></asp:RadioButtonList>
            <br />
            <asp:Button ID="btnNextQuestion" runat="server" Text="Next" OnClick="btnNextQuestion_Click" />

            <!-- Validation -->
            <asp:RequiredFieldValidator
                ID="RequiredGender"
                runat="server" 
                ErrorMessage="Gender is required"
                ControlToValidate="rblInput"
                Display="Static">
            </asp:RequiredFieldValidator>

        </div>
    </form>
</body>
</html>
