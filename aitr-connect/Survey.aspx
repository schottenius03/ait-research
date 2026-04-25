<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Survey.aspx.cs" Inherits="aitr_connect.Survey" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Survey - AITR Connect</title>
    <link href="Styles/Survey_styles.css" rel="stylesheet" type="text/css" />
</head>
<body>
    <form id="form1" runat="server">
        <div class="main-container">
            <div class="nav-header">
                <asp:Button ID="btnBackToDefault" runat="server" Text="← Go back" CssClass="btn btn-back" OnClick="btnBackToDefault_Click" />
            </div>

            <div class="header-area">
                <asp:Label ID="lblTitle" runat="server" CssClass="header-title">
                    Question <asp:Label ID="lblQuestionNumber" runat="server" Text=""></asp:Label>
                </asp:Label>
            </div>
            
            <div class="question-content">
                <asp:PlaceHolder ID="phQuestionArea" runat="server"></asp:PlaceHolder>
            </div>

            <asp:Label ID="lblErrorMessage" runat="server" CssClass="error-label" Visible="false"></asp:Label>

            <asp:LinkButton ID="btnSkip" CssClass="btn-skip" runat="server" OnClick="btnSkip_Click">Skip this question</asp:LinkButton>

            <div class="action-area">
                <asp:Button ID="btnNextQuestion" runat="server" Text="Next" CssClass="btn btn-primary" OnClick="btnNextQuestion_Click" />
            </div>
        </div>
    </form>
</body>
</html>