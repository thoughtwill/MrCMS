namespace MrCMS.Jobs.RecurringJob;

public class ClearFormEntriesJob : MrCMSRecurringJob
{

    public override void OnAddOrUpdate(string cron)
    {
        Hangfire.RecurringJob.AddOrUpdate<IClearFormEntries>(Id, service => service.Execute(), cron);
    }
}