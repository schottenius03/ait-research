using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;

namespace aitr_connect
{
    public class PageBase : System.Web.UI.Page
    {
        /// <summary>
        /// Centralized connection string retrieved from AppConstant.
        /// </summary>
        public string CurrentConnectionString
        {
            get { return AppConstant.Connection.DevConnectionString; }
        }

        /// <summary>
        /// Defines the active survey ID for the Data Driven Architecture (DDA).
        /// </summary>
        public int CurrentSurveyID
        {
            get
            {
                // Check if SurveyID is stored in session
                if (Session[AppConstant.SessionNameList.strSurveyID] != null)
                    return Convert.ToInt32(Session[AppConstant.SessionNameList.strSurveyID]);

                return 1; // Fallback to ID 1
            }
        }

        /// <summary>
        /// Validates that the database connection is open and available.
        /// </summary>
        /// <returns>True if connection is successful</returns>
        public bool PageValid()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(CurrentConnectionString))
                {
                    conn.Open();
                    return true;
                }
            }
            catch (Exception ex)
            {
                // Store error message for the ErrorPage to display
                Session[AppConstant.SessionNameList.strErroMessage] = "Database Connection Error: " + ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Clears all survey-related sessions upon completion or cancellation.
        /// </summary>
        public void ClearSurveySessions()
        {
            Session.Remove(AppConstant.SessionNameList.strIsSurveyActive);
            Session.Remove(AppConstant.SessionNameList.strQuestionIndex);
            Session.Remove(AppConstant.SessionNameList.strRespondentID);
            Session.Remove(AppConstant.SessionNameList.strSessionID);
        }
    }
}