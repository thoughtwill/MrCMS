using MrCMS.TextSearch.Services;

namespace MrCMS.Jobs.RecurringJob;

public class RefreshTextSearchIndexJob : MrCMSRecurringJob
{
    public override void OnAddOrUpdate(string cron)
    {
        Hangfire.RecurringJob.AddOrUpdate<IRefreshTextSearchIndex>(Id, service => service.Refresh(), cron);
    }
}