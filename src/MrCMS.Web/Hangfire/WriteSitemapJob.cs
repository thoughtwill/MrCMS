using Hangfire;
using MrCMS.Jobs;
using MrCMS.Services.Sitemaps;

namespace MrCMS.Web.Hangfire;

public class WriteSitemapJob : MrCMSRecurringJob
{
    public override void OnAddOrUpdate(string cron)
    {
        RecurringJob.AddOrUpdate<ISitemapService>(Id, service => service.WriteSitemap(), cron);
    }
}