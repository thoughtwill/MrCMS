namespace MrCMS.Jobs.RecurringJob;

public class DeleteExpiredLogsJob : MrCMSRecurringJob
{
    public override void OnAddOrUpdate(string cron)
    {
        Hangfire.RecurringJob.AddOrUpdate<IDeleteExpiredLogsTask>(Id, service => service.Execute(), cron);
    }
}