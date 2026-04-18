using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace aitr_connect
{
    public class SurveyQuestion
    {
        public int ID { get; set; }
        public string Text { get; set; }
        public string Type { get; set; }
        public int MinSelections { get; set; }
        public int MaxSelections { get; set; }
    }
}