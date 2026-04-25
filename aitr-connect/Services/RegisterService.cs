using System;
using System.Data.SqlClient;

namespace aitr_connect.Services
{
    public class RegisterValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class RegisterQuestion
    {
        public int ID { get; set; }
        public string Text { get; set; }
        public string Type { get; set; }
        public bool IsLastQuestion { get; set; }
    }

    public class RegisterService
    {
        /// <summary>
        /// get question to check if it's the last question
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="questionID"></param>
        /// <returns></returns>
        public RegisterQuestion GetQuestionByID(string connectionString, int questionID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                // check if there's a higher ID 
                string sql = @"
                    SELECT questionID, questionText, questionType,
                    CASE 
                        WHEN NOT EXISTS (SELECT 1 FROM Question WHERE questionID > @qID) THEN 1 
                        ELSE 0 
                    END as IsLast
                    FROM Question 
                    WHERE questionID = @qID";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@qID", questionID);
                    try
                    {
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new RegisterQuestion
                                {
                                    ID = Convert.ToInt32(reader["questionID"]),
                                    Text = reader["questionText"].ToString(),
                                    Type = reader["questionType"].ToString().Trim(),
                                    IsLastQuestion = Convert.ToBoolean(reader["IsLast"])
                                };
                            }
                        }
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Create new respondent that has not entered the register page from the survey
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public int CreateNewRespondentWithSession(string connectionString, string ipAddress)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sqlResp = "INSERT INTO Respondent (IsAnonymous) VALUES (1); SELECT SCOPE_IDENTITY();";
                SqlCommand cmdResp = new SqlCommand(sqlResp, conn);
                int rID = Convert.ToInt32(cmdResp.ExecuteScalar());

                string sqlSess = "INSERT INTO ResearchSession (respondentID, sessionDateTime, ipAddress, isCompleted) VALUES (@rID, GETDATE(), @ip, 0)";
                SqlCommand cmdSess = new SqlCommand(sqlSess, conn);
                cmdSess.Parameters.AddWithValue("@rID", rID);
                cmdSess.Parameters.AddWithValue("@ip", ipAddress);
                cmdSess.ExecuteNonQuery();

                return rID;
            }
        }

        /// <summary>
        /// validate the user input 
        /// </summary>
        /// <param name="connString"></param>
        /// <param name="qID"></param>
        /// <param name="textAnswer"></param>
        /// <returns></returns>
        public RegisterValidationResult ValidateUserSubmission(string connString, int qID, string textAnswer)
        {
            RegisterQuestion question = GetQuestionByID(connString, qID);
            if (question == null) return new RegisterValidationResult { IsValid = true };

            if (string.IsNullOrWhiteSpace(textAnswer))
            {
                return new RegisterValidationResult { IsValid = false, ErrorMessage = "This field is required." };
            }

            switch (question.Type)
            {
                case AppConstant.QuestionTypes.TextBoxAlpha:
                    if (!System.Text.RegularExpressions.Regex.IsMatch(textAnswer, @"^[a-zA-Z\s\-åäöÅÄÖ]+$"))
                    {
                        return new RegisterValidationResult { IsValid = false, ErrorMessage = "Names can only contain letters." };
                    }
                    break;

                case AppConstant.QuestionTypes.TextBoxNumeric:
                case "TextBox_Numeric":
                    if (!System.Text.RegularExpressions.Regex.IsMatch(textAnswer, @"^\d{1,10}$"))
                    {
                        return new RegisterValidationResult { IsValid = false, ErrorMessage = "Please enter a valid number (max 10 digits)." };
                    }
                    break;

                case "TextBox_Date":
                case "TextBox":
                    if (question.ID == AppConstant.QuestionConfig.intBirthDateID)
                    {
                        if (DateTime.TryParse(textAnswer, out DateTime parsedDate))
                        {
                            DateTime minDate = new DateTime(1900, 1, 1);
                            DateTime maxDate = new DateTime(2026, 1, 1);

                            if (parsedDate < minDate || parsedDate > maxDate)
                            {
                                return new RegisterValidationResult { IsValid = false, ErrorMessage = "Date must be between 1900-01-01 and 2026-01-01." };
                            }
                        }
                        else
                        {
                            return new RegisterValidationResult { IsValid = false, ErrorMessage = "Invalid format. Please use YYYY-MM-DD." };
                        }
                    }
                    break;
            }

            return new RegisterValidationResult { IsValid = true };
        }

        /// <summary>
        /// update column in respondent table
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="respondentID"></param>
        /// <param name="questionID"></param>
        /// <param name="answer"></param>
        public void UpdateRespondentData(string connectionString, int respondentID, int questionID, string answer)
        {
            string columnName = "";
            switch (questionID)
            {
                case AppConstant.QuestionConfig.intFirstNameID:
                    columnName = "firstName"; break;
                case AppConstant.QuestionConfig.intLastNameID:
                    columnName = "lastName"; break;
                case AppConstant.QuestionConfig.intBirthDateID:
                    columnName = "dateOfBirth"; break;
                case AppConstant.QuestionConfig.intPhoneID:
                    columnName = "phoneNumber"; break;
            }

            if (!string.IsNullOrEmpty(columnName))
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string sql = $"UPDATE Respondent SET {columnName} = @val WHERE respondentID = @rID";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        if (columnName == "dateOfBirth")
                        {
                            if (DateTime.TryParse(answer, out DateTime parsedDate))
                            {
                                cmd.Parameters.AddWithValue("@val", parsedDate.Date);
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue("@val", DBNull.Value);
                            }
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@val", (object)answer ?? DBNull.Value);
                        }

                        cmd.Parameters.AddWithValue("@rID", respondentID);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        /// <summary>
        /// mark respondent as non-anonymous anymore
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="respondentID"></param>
        /// <returns></returns>
        public bool MarkAsRegistered(string connectionString, int respondentID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                // We update both tables to ensure data integrity
                string sql = @"
                    UPDATE Respondent SET IsAnonymous = 0 WHERE respondentID = @rID;
                    UPDATE ResearchSession SET isCompleted = 1 WHERE respondentID = @rID;";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@rID", respondentID);
                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
    }
}