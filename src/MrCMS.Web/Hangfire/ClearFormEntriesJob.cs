using Hangfire;
using MrCMS.Tasks;

namespace MrCMS.Web.Hangfire;

public class ClearFormEntriesJob : MrCMSRecurringJob
{
    public override string Id => "Clear Form Entries";

    public override void OnAddOrUpdate(string cron)
    {
        RecurringJob.AddOrUpdate<IClearFormEntries>(Id, service => service.Execute(), cron);
    }
}