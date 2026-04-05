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
        public bool PageValid()
        {
            try
            {
                // tea - straw - connect - poke - get soda - consume
                string configKey = ConfigurationManager.ConnectionStrings["KailingConnectionString"].ConnectionString;
                string activeConnectionString = "";

                // check for correct enviroment 
                switch (configKey.ToUpper())
                {
                    case "DEV":
                        activeConnectionString = AppConstant.Connection.DevConnectionString;
                        break;
                    case "TEST":
                        activeConnectionString = AppConstant.Connection.TestConnectionString;
                        break;
                    case "PROD":
                        activeConnectionString = AppConstant.Connection.ProdConnectionString;
                        break;
                    default:
                        activeConnectionString = ""; // no available enoviroment
                        break;
                }

                // set assigned key to variable
                this.CurrentConnectionString = activeConnectionString;

                // no available enviroment 
                if (string.IsNullOrEmpty(this.CurrentConnectionString))
                {
                    Session[AppConstant.SessionNameList.strErroMessage] = "No valid environment!!! Contact admin";
                    return false;
                }

                // initiate connection
                SqlConnection myconn = new SqlConnection();
                myconn.ConnectionString = activeConnectionString;

                // enviroment is okay
                return true;
            }
            // specific error message
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