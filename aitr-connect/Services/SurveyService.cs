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

    /// <summary>
    /// Class to store values for current question 
    /// </summary>
    public class SurveyQuestion
    {
        public int ID { get; set; }
        public string Text { get; set; }
        public string Type { get; set; }
        public int MinSelections { get; set; }
        public int MaxSelections { get; set; }
    }

    /// <summary>
    /// Class to store the respondent's answer
    /// </summary>
    public class QuestionOption
    {
        public int OptionID { get; set; }
        public string OptionText { get; set; }
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

        /// <summary>
        /// Gets a specific question from the DB based on its display order.
        /// </summary>
        public SurveyQuestion GetQuestionByOrder(string connectionString, int currentOrder, int surveyID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                // get question based on displayOrder
                string sql = @"SELECT q.questionID, q.questionText, q.questionType, q.minSelections, q.maxSelections 
                       FROM Question q 
                       JOIN SurveyQuestion sq ON q.questionID = sq.questionID 
                       WHERE sq.displayOrder = @order AND sq.surveyID = @sID AND sq.isActive = 1";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@order", currentOrder);
                    cmd.Parameters.AddWithValue("@sID", surveyID);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new SurveyQuestion
                            {
                                ID = Convert.ToInt32(reader["questionID"]),
                                Text = reader["questionText"].ToString(),
                                Type = reader["questionType"].ToString(),
                                MinSelections = Convert.ToInt32(reader["minSelections"]),
                                MaxSelections = Convert.ToInt32(reader["maxSelections"])
                            };
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Gets all available options for a specific question.
        /// </summary>
        public List<QuestionOption> GetOptionsByQuestionID(string connectionString, int questionID)
        {
            List<QuestionOption> options = new List<QuestionOption>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT optionID, optionText FROM [Option] WHERE questionID = @qID AND isActive = 1";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@qID", questionID);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            options.Add(new QuestionOption
                            {
                                OptionID = Convert.ToInt32(reader["optionID"]),
                                OptionText = reader["optionText"].ToString()
                            });
                        }
                    }
                }
            }
            return options;
        }

        /// <summary>
        /// Finds the next main question order, skipping sub-questions defined in QuestionRule.
        /// </summary>
        public int GetNextMainQuestionOrder(string connectionString, int currentOrder, int surveyID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                // get next question that is not a sub question
                string sql = @"
            SELECT MIN(sq.displayOrder) 
            FROM SurveyQuestion sq 
            WHERE sq.surveyID = @sID 
            AND sq.displayOrder > @currentOrder 
            AND sq.isActive = 1
            AND sq.questionID NOT IN (SELECT childQuestionID FROM QuestionRule)";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    // Koppla parametrarna till SQL-frågan
                    cmd.Parameters.AddWithValue("@sID", surveyID);
                    cmd.Parameters.AddWithValue("@currentOrder", currentOrder);

                    object result = cmd.ExecuteScalar();

                    return (result != DBNull.Value && result != null) ? Convert.ToInt32(result) : -1;
                }
            }
        }
        
        /// <summary>
        /// store answer to ResponseAnswer table
        /// </summary>
        public void SaveAnswer(string connectionString, int sessionID, int questionID, int? optionID, string textAnswer)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = @"INSERT INTO ResponseAnswer (sessionID, questionID, optionID, textAnswer, dateRecorded) 
                       VALUES (@sID, @qID, @oID, @txt, GETDATE())";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@sID", sessionID);
                    cmd.Parameters.AddWithValue("@qID", questionID);

                    // handle Null values 
                    cmd.Parameters.AddWithValue("@oID", (object)optionID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@txt", (object)textAnswer ?? DBNull.Value);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// get min and max selection of the question
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="questionID"></param>
        /// <returns></returns>
        public (int Min, int Max) GetQuestionRequirements(string connectionString, int questionID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT minSelections, maxSelections FROM Question WHERE questionID = @qID";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@qID", questionID);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return (Convert.ToInt32(reader["minSelections"]), Convert.ToInt32(reader["maxSelections"]));
                        }
                    }
                }
            }
            return (0, 1); 
        }

        /// <summary>
        /// verify if sub question exist 
        /// </summary>
        public int? GetSubQuestionOrder(string connectionString, int optionID, int surveyID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                // search using trigger for sub question existence
                string sql = @"
            SELECT sq.displayOrder 
            FROM QuestionRule qr
            JOIN SurveyQuestion sq ON qr.childQuestionID = sq.questionID
            WHERE qr.parentOptionID = @oID AND sq.surveyID = @sID AND sq.isActive = 1";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@oID", optionID);
                    cmd.Parameters.AddWithValue("@sID", surveyID);

                    object result = cmd.ExecuteScalar();
                    return (result != null && result != DBNull.Value) ? (int?)Convert.ToInt32(result) : null;
                }
            }
        }
    }
}