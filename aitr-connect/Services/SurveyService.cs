using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace aitr_connect.Services
{
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
    }
}