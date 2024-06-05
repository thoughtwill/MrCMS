using Hangfire;
using MrCMS.Jobs;

namespace MrCMS.Web.Hangfire;

public class PublishScheduledWebpagesJob : MrCMSRecurringJob
{
    public override void OnAddOrUpdate(string cron)
    {
        RecurringJob.AddOrUpdate<IPublishScheduledWebpagesTask>(Id, service => service.Execute(), cron);
    }
}