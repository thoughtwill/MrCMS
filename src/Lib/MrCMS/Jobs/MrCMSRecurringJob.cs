using MrCMS.Helpers;

namespace MrCMS.Jobs;

public abstract class MrCMSRecurringJob
{
    public virtual string Id => GetType().Name;
    public virtual string DisplayName => Id.BreakUpString();
    public abstract void OnAddOrUpdate(string cron);
}