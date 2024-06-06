using MrCMS.Services.Sitemaps;

namespace MrCMS.Jobs.RecurringJob;

public class WriteSitemapJob : MrCMSRecurringJob
{
    public override void OnAddOrUpdate(string cron)
    {
        Hangfire.RecurringJob.AddOrUpdate<ISitemapService>(Id, service => service.WriteSitemap(), cron);
    }
}