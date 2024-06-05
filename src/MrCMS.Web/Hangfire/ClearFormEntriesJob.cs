using Hangfire;
using MrCMS.Jobs;

namespace MrCMS.Web.Hangfire;

public class ClearFormEntriesJob : MrCMSRecurringJob
{

    public override void OnAddOrUpdate(string cron)
    {
        RecurringJob.AddOrUpdate<IClearFormEntries>(Id, service => service.Execute(), cron);
    }
}