namespace MrCMS.Jobs.RecurringJob;

public class RefreshTextSearchIndexJob : MrCMSRecurringJob
{
    public override void OnAddOrUpdate(string cron)
    {
        Hangfire.RecurringJob.AddOrUpdate<IRefreshTextSearchIndexTask>(Id, service => service.Refresh(), cron);
    }
}