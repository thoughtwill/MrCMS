using Hangfire;
using MrCMS.Tasks;

namespace MrCMS.Web.Hangfire;

public class DeleteExpiredLogsJob : MrCMSRecurringJob
{
    public override string Id => "Delete Expired Logs";

    public override void OnAddOrUpdate(string cron)
    {
        RecurringJob.AddOrUpdate<IDeleteExpiredLogsTask>(Id, service => service.Execute(), cron);
    }
}