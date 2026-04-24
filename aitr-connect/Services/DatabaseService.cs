using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace aitr_connect.Services
{
    public class DatabaseService
    {
        /// <summary>
        /// Verify what connection is supposted to connect.
        /// </summary>
        /// <returns></returns>
        public string GetActiveConnectionString()
        {
            // tea - straw - connect - poke - get soda - consume
            string configKey = ConfigurationManager.ConnectionStrings["KailingConnectionString"].ConnectionString;

            switch (configKey.ToUpper())
            {
                case "DEV": return AppConstant.Connection.DevConnectionString;
                case "TEST": return AppConstant.Connection.TestConnectionString;
                case "PROD": return AppConstant.Connection.ProdConnectionString;
                default: return "";
            }
        }
    }
}