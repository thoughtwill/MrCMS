using Hangfire;
using MrCMS.TextSearch.Services;

namespace MrCMS.Jobs;

public class RefreshTextIndexes : MrCMSAdHocJob
{
    public override string QueueJob()
    {
        return BackgroundJob.Enqueue<IRefreshTextSearchIndex>(x => x.Refresh());
    }
}