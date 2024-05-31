namespace MrCMS.Tasks;

public abstract class MrCMSRecurringJob
{
    public abstract string Id { get; }
    public abstract void OnAddOrUpdate(string cron);
}

