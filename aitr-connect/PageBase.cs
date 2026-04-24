using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace aitr_connect
{
    public class PageBase : System.Web.UI.Page
    {
        // declare connection key
        public string CurrentConnectionString { get; set; }

        /// <summary>
        /// Validte which enviroment to run with the correct connection string through DatabaseService.
        /// </summary>
        /// <returns></returns>
        public bool PageValid()
        {
            try
            {
                // create instance 
                var dbService = new aitr_connect.Services.DatabaseService();

                // calculate the correct connection string 
                this.CurrentConnectionString = dbService.GetActiveConnectionString();

                // enviroment is not valid 
                if (string.IsNullOrEmpty(this.CurrentConnectionString))
                {
                    Session[AppConstant.SessionNameList.strErroMessage] = "No valid environment!!! Contact admin";
                    return false;
                }

                // Enviroment is okay
                return true;
            }
            catch (InvalidOperationException)
            {
                Session[AppConstant.SessionNameList.strErroMessage] = "Internal operation error!!! Contact admin";
                return false;
            }
            catch (ConfigurationErrorsException)
            {
                Session[AppConstant.SessionNameList.strErroMessage] = "Internal configuration error!!! Contact admin";
                return false;
            }
            catch (SqlException)
            {
                Session[AppConstant.SessionNameList.strErroMessage] = "Database general error!!! Try again later.";
                return false;
            }
            catch (Exception)
            {
                Session[AppConstant.SessionNameList.strErroMessage] = "General system error!!! Try again later.";
                return false;
            }
        }
    }
}