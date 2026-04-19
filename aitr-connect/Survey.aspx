<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Survey.aspx.cs" Inherits="aitr_connect.Survey" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Survey</title>
    <style>
        /* CSS for button to skip question */
        .hover-underline {
            color: #808080;
            text-decoration: none; 
            font-size: 16px;
            display: inline-block;
        }

        .hover-underline:hover {
            text-decoration: underline; 
            color: #000; 
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <!-- Design -->
            <asp:Button ID="btnBackToDefault" runat="server" Text="< Go back" BorderStyle="None" BackColor="White" BorderWidth="0" OnClick="btnBackToDefault_Click" />
            <br />
            <br />
            <asp:Label ID="lblTitle" runat="server" Text="Question " Font-Size="XX-Large" Font-Bold="True" ForeColor="#003399" ></asp:Label>
            <asp:Label ID="lblQuestionNumber" runat="server" Text="" Font-Size="XX-Large" Font-Bold="True" ForeColor="#003399" ></asp:Label>
            <br />
            <br />

            <!-- Question -->
            <asp:PlaceHolder ID="phQuestionArea" runat="server"></asp:PlaceHolder>
            <br />
            <asp:LinkButton ID="btnSkip" CssClass="hover-underline" runat="server" OnClick="btnSkip_Click">Skip this question</asp:LinkButton>
            <br />
            <br />
            <asp:Button ID="btnNextQuestion" runat="server" Text="Next" OnClick="btnNextQuestion_Click" />

        </div>
    </form>
</body>
</html>
