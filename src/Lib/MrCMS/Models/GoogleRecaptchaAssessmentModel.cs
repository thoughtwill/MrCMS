using System;
using System.Collections.Generic;

namespace MrCMS.Models
{
    public class GoogleRecaptchaAssessmentModel
    {
        public GoogleRecaptchaTokenModel TokenProperties { get; set; }
        public GoogleRecaptchaEventModel Event { get; set; }
        public GoogleRecaptchaRiskAnalysisModel RiskAnalysis { get; set; }
        public string Name { get; set; }
    }

    public class GoogleRecaptchaEventModel
    {
        public string Token { get; set; }
        public string SiteKey { get; set; }
        public string ExpectedAction { get; set; }
    } 
    
    public class GoogleRecaptchaRiskAnalysisModel
    {
        public double Score { get; set; }
        public List<string> Reasons { get; set; }
    }

    public class GoogleRecaptchaTokenModel
    {
        public bool Valid { get; set; }
        public string Hostname { get; set; }
        public string Action { get; set; }
        public DateTime CreateTime { get; set; }
    }

}
