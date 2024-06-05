using Hangfire;
using MrCMS.Jobs;
using MrCMS.TextSearch.Services;

namespace MrCMS.Web.Hangfire;

public class RefreshTextSearchIndexJob : MrCMSRecurringJob
{
    public override void OnAddOrUpdate(string cron)
    {
        RecurringJob.AddOrUpdate<IRefreshTextSearchIndex>(Id, service => service.Refresh(), cron);
    }
}