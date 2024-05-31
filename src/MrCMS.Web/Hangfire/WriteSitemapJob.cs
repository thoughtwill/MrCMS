using Hangfire;
using MrCMS.Services.Sitemaps;
using MrCMS.Tasks;

namespace MrCMS.Web.Hangfire;

public class WriteSitemapJob : MrCMSRecurringJob
{
    public override string Id => "Write Sitemap";

    public override void OnAddOrUpdate(string cron)
    {
        RecurringJob.AddOrUpdate<ISitemapService>(Id, service => service.WriteSitemap(), cron);
    }
}