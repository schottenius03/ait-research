using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace aitr_connect
{
    public class AppConstant
    {
        // DB connection management for multiple enviroments
        public class Connection
        {
            public const String DevConnectionString = "Data Source=SQL8012.site4now.net;Initial Catalog=db_9ab8b7_126dda14621;User Id=db_9ab8b7_126dda14621_admin;Password=DG5py76V;";
            public const String TestConnectionString = "";
            public const String ProdConnectionString = "";
        }

        // URLs page redirection 
        public class PageCatalog
        {
            public const string strDefaultPage = "~/Default.aspx";
            public const string strSearchPage = "~/Search.aspx";
            public const string strRegisterPage = "~/Register.aspx";
            public const string strSurveyPage = "~/Survey.aspx";
            public const string strErrorPage = "~/ErrorPage.aspx";
        }

        // map dataTypes from DB
        public class QuestionTypes
        {
            public const int RegisterYesOptionID = 56; // yes register 
            public const int RegisterNoOptionID = 57;  // no register 
            public const string RadioButtonRegister = "RadioButton_Register";
            public const string RadioButton = "RadioButton";
            public const string CheckBox = "CheckBox";
            public const string DropDown = "DropDown";
            public const string TextBoxEmail = "TextBox_Email";
            public const string TextBoxNumeric = "TextBox_Numeric_4";
            public const string TextBoxAlpha = "TextBox_Alpha";
        }

        // Session def
        public class SessionNameList
        {
            // survey status 
            public const string strIsSurveyActive = "isSurveyActive";
            public const string strErroMessage = "strErroMessage";

            public const string strQuestionIndex = "questionIndex";
            public const string strUserAnswers = "respondentAnswers";

            public const string strRespondentID = "currentRespondentID";
            public const string strSessionID = "currentSessionID";
            public const string strQuestionID = "currentQuestionID";
            public const string strQuestionType = "currentQuestionType";
        }
    }
}