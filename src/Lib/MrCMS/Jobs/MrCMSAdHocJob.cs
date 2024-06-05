using MrCMS.Helpers;

namespace MrCMS.Jobs;

public abstract class MrCMSAdHocJob
{
    public virtual string Id => GetType().Name;
    public virtual string DisplayName => Id.BreakUpString();

    /// <summary>
    /// Trigger the job and return the job id
    /// </summary>
    /// <returns>JobId</returns>
    public abstract string QueueJob();
}