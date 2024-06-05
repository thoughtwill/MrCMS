namespace MrCMS.Web.Admin.Models.Jobs;

public class SetupRecurringJobModel
{
    public string Id { get; set; }
    public string DisplayName { get; set; }
    public string Cron { get; set; }
}