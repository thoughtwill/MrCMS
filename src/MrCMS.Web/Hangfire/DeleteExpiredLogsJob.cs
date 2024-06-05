using Hangfire;
using MrCMS.Jobs;

namespace MrCMS.Web.Hangfire;

public class DeleteExpiredLogsJob : MrCMSRecurringJob
{
    public override void OnAddOrUpdate(string cron)
    {
        RecurringJob.AddOrUpdate<IDeleteExpiredLogsTask>(Id, service => service.Execute(), cron);
    }
}