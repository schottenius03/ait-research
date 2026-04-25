using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

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

    /// <summary>
    /// Class to store Validation values
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
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
        /// <param name="connectionString"></param>
        /// <param name="currentOrder"></param>
        /// <param name="surveyID"></param>
        /// <returns></returns>
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
        /// <param name="connectionString"></param>
        /// <param name="questionID"></param>
        /// <returns></returns>
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
        /// <param name="connectionString"></param>
        /// <param name="currentOrder"></param>
        /// <param name="surveyID"></param>
        /// <returns></returns>
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
                    // map parameters to query
                    cmd.Parameters.AddWithValue("@sID", surveyID);
                    cmd.Parameters.AddWithValue("@currentOrder", currentOrder);

                    object result = cmd.ExecuteScalar();

                    return (result != DBNull.Value && result != null) ? Convert.ToInt32(result) : -1;
                }
            }
        }

        /// <summary>
        /// Save answer and update ResponseAnswer table along with RespondentID 
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="sessionID"></param>
        /// <param name="questionID"></param>
        /// <param name="optionID"></param>
        /// <param name="textAnswer"></param>
        public void SaveAnswer(string connectionString, int sessionID, int questionID, int? optionID, string textAnswer)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // get respondentID from session
                int respondentID = 0;
                string resSql = "SELECT respondentID FROM ResearchSession WHERE sessionID = @sID";
                using (SqlCommand cmdRes = new SqlCommand(resSql, conn))
                {
                    cmdRes.Parameters.AddWithValue("@sID", sessionID);
                    object resResult = cmdRes.ExecuteScalar();
                    if (resResult != null && resResult != DBNull.Value)
                    {
                        respondentID = Convert.ToInt32(resResult);
                    }
                }

                // get questionType
                string typeSql = "SELECT questionType FROM Question WHERE questionID = @qID";
                string qType = "";
                using (SqlCommand cmdType = new SqlCommand(typeSql, conn))
                {
                    cmdType.Parameters.AddWithValue("@qID", questionID);
                    object result = cmdType.ExecuteScalar();
                    qType = result != null ? result.ToString() : "";
                }

                // save to ResponseAnswer with RespondentID
                string sql = @"INSERT INTO ResponseAnswer (sessionID, respondentID, questionID, optionID, textAnswer, dateRecorded) 
                        VALUES (@sID, @rID, @qID, @oID, @txt, GETDATE())";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@sID", sessionID);
                    cmd.Parameters.AddWithValue("@rID", respondentID > 0 ? (object)respondentID : DBNull.Value);
                    cmd.Parameters.AddWithValue("@qID", questionID);

                    // handle Null values 
                    cmd.Parameters.AddWithValue("@oID", (object)optionID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@txt", (object)textAnswer ?? DBNull.Value);

                    cmd.ExecuteNonQuery();
                }

                // email to update respondent table 
                if (qType == AppConstant.QuestionTypes.TextBoxEmail && !string.IsNullOrEmpty(textAnswer))
                {
                    // update anonymous status
                    string sqlEmail = @"UPDATE Respondent 
                                SET email = @email, IsAnonymous = 0 
                                WHERE respondentID = @rID";

                    using (SqlCommand cmdEmail = new SqlCommand(sqlEmail, conn))
                    {
                        cmdEmail.Parameters.AddWithValue("@email", textAnswer);
                        cmdEmail.Parameters.AddWithValue("@rID", respondentID);
                        cmdEmail.ExecuteNonQuery();
                    }
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
        /// Verify if sub question exist
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="optionID"></param>
        /// <param name="surveyID"></param>
        /// <returns></returns>
        public int? GetSubQuestionOrder(string connectionString, int optionID, int surveyID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                // get sub question for the parent question
                string sql = @"
            SELECT sq.displayOrder 
            FROM QuestionRule qr
            JOIN SurveyQuestion sq ON qr.childQuestionID = sq.questionID
            WHERE qr.triggerOptionID = @oID 
            AND sq.surveyID = @sID 
            AND sq.isActive = 1";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@oID", optionID);
                    cmd.Parameters.AddWithValue("@sID", surveyID);

                    object result = cmd.ExecuteScalar();
                    return (result != null && result != DBNull.Value) ? (int?)Convert.ToInt32(result) : null;
                }
            }
        }

        /// <summary>
        /// Validating certain requirments needed from DB
        /// </summary>
        /// <param name="connString"></param>
        /// <param name="qID"></param>
        /// <param name="count"></param>
        /// <param name="textAnswer"></param>
        /// <returns></returns>
        public ValidationResult ValidateUserSubmission(string connString, int qID, int count, string textAnswer)
        {
            var req = GetQuestionRequirements(connString, qID);

            // get next question by ID 
            SurveyQuestion question = null;
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                string sql = "SELECT questionID, questionType FROM Question WHERE questionID = @qID";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@qID", qID);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            question = new SurveyQuestion
                            {
                                ID = Convert.ToInt32(reader["questionID"]),
                                Type = reader["questionType"].ToString()
                            };
                        }
                    }
                }
            }

            if (question == null) return new ValidationResult { IsValid = true };

            // compare min max 
            if (count < req.Min)
                return new ValidationResult { IsValid = false, ErrorMessage = $"Please select at least {req.Min} options." };
            if (count > req.Max)
                return new ValidationResult { IsValid = false, ErrorMessage = $"Maximum {req.Max} options allowed." };

            // specific requirements for textBox
            if (!string.IsNullOrEmpty(textAnswer))
            {
                switch (question.Type)
                {
                    case AppConstant.QuestionTypes.TextBoxAlpha:
                        if (!System.Text.RegularExpressions.Regex.IsMatch(textAnswer, @"^[a-zA-Z\s\-]+$"))
                        {
                            return new ValidationResult { IsValid = false, ErrorMessage = "Suburb name can only contain letters." };
                        }
                        break;

                    case AppConstant.QuestionTypes.TextBoxNumeric:
                        if (!System.Text.RegularExpressions.Regex.IsMatch(textAnswer, @"^\d{4}$"))
                        {
                            return new ValidationResult { IsValid = false, ErrorMessage = "Postcode must be exactly 4 digits." };
                        }
                        break;

                    case AppConstant.QuestionTypes.TextBoxEmail:
                        if (!System.Text.RegularExpressions.Regex.IsMatch(textAnswer, @"^[^@\s]+@[^@\s]+\.[a-zA-Z]{2,}$"))
                        {
                            return new ValidationResult { IsValid = false, ErrorMessage = "Please enter a valid email (e.g. name@domain.com). Numbers are not allowed in the domain suffix." };
                        }
                        break;
                }
            }

            return new ValidationResult { IsValid = true };
        }

        /// <summary>
        /// Mark session as completed
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="sessionID"></param>
        public void CompleteSession(string connectionString, int sessionID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = "UPDATE ResearchSession SET isCompleted = 1 WHERE sessionID = @sID";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@sID", sessionID);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Update respondents status if anonymous or not 
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="sessionID"></param>
        /// <param name="isAnonymous"></param>
        public void UpdateAnonymousStatus(string connectionString, int sessionID, bool isAnonymous)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                // get respondent thorugh researchSession
                string sql = @"UPDATE Respondent 
                       SET IsAnonymous = @isAnon 
                       WHERE respondentID = (SELECT respondentID FROM ResearchSession WHERE sessionID = @sID)";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@isAnon", isAnonymous ? 1 : 0);
                    cmd.Parameters.AddWithValue("@sID", sessionID);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}