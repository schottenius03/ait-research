using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace aitr_connect.Services
{
    /// <summary>
    /// Class to store IDs for created session.
    /// </summary>
    public class SurveySessionResult
    {
        public int RespondentID { get; set; }
        public int SessionID { get; set; }
    }
    public class SurveyService
    {

        /// <summary>
        /// Verify session has started and update the status. 
        /// </summary>
        /// <param name="sessionValue"></param>
        /// <returns></returns>
        public bool IsSurveyAccessValid(object sessionValue)
        {
            if (sessionValue == null)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// create new respondent and session in DB and return the IDs.
        /// </summary>
        /// <param name="connectionString">Connection string to DB.</param>
        /// <param name="ipAddress">Respodnent IP Address.</param>
        public SurveySessionResult CreateNewSurveySession(string connectionString, string ipAddress)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // create respondent 
                string sqlRespondent = "INSERT INTO Respondent DEFAULT VALUES; SELECT SCOPE_IDENTITY();";
                SqlCommand cmdResp = new SqlCommand(sqlRespondent, conn);
                int rID = Convert.ToInt32(cmdResp.ExecuteScalar());

                // create new session
                string sqlSession = "INSERT INTO ResearchSession (respondentID, sessionDateTime, ipAddress) VALUES (@rID, GETDATE(), @ip); SELECT SCOPE_IDENTITY();";
                SqlCommand cmdSess = new SqlCommand(sqlSession, conn);
                cmdSess.Parameters.AddWithValue("@rID", rID);
                cmdSess.Parameters.AddWithValue("@ip", ipAddress);

                int sID = Convert.ToInt32(cmdSess.ExecuteScalar());

                // return as an object
                return new SurveySessionResult
                {
                    RespondentID = rID,
                    SessionID = sID
                };
            }
        }
    }
}