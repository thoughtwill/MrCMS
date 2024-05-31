using Hangfire;
using MrCMS.Tasks;
using MrCMS.TextSearch.Services;

namespace MrCMS.Web.Hangfire;

public class RefreshTextSearchIndexJob : MrCMSRecurringJob
{
    public override string Id => "Refresh Text Search Index";

    public override void OnAddOrUpdate(string cron)
    {
        RecurringJob.AddOrUpdate<IRefreshTextSearchIndex>(Id, service => service.Refresh(), cron);
    }
}