namespace MrCMS.Jobs.RecurringJob;

public class PublishScheduledWebpagesJob : MrCMSRecurringJob
{
    public override void OnAddOrUpdate(string cron)
    {
        Hangfire.RecurringJob.AddOrUpdate<IPublishScheduledWebpagesTask>(Id, service => service.Execute(), cron);
    }
}