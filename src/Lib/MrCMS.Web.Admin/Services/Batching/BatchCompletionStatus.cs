namespace MrCMS.Web.Admin.Services.Batching
{
    public struct BatchCompletionStatus
    {
        public int Total { get; set; }

        public int Pending { get; set; }
        public int Failed { get; set; }
        public int Succeeded { get; set; }

        public string AverageTimeTaken { get; set; }
        public string TimeTaken { get; set; }
        public string EstimatedTimeRemaining { get; set; }

        public string PercentageCompleted
        {
            get
            {
                if (Total == 0)
                    return "100%";

                return (((decimal)Completed / (decimal)Total) * 100).ToString("0.00") + "%";
            }
        }

        public int Completed
        {
            get { return Failed + Succeeded; }
        }

        public string FullStatus => $"{PercentageCompleted} ({Completed} of {Total})";
    }
}