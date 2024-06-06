namespace MrCMS.Jobs.RecurringJob;

public class SendQueuedMessagesJob : MrCMSRecurringJob
{
    public override void OnAddOrUpdate(string cron)
    {
        Hangfire.RecurringJob.AddOrUpdate<ISendQueuedMessagesTask>(Id, service => service.Execute(), cron);
    }
}