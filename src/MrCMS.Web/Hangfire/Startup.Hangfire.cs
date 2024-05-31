using System;
using Hangfire;
using Microsoft.AspNetCore.Builder;

namespace MrCMS.Web.Hangfire
{
    public static class HangfireJobs
    {
        public static void RegisterJobs(this IApplicationBuilder app, IServiceProvider serviceProvider)
        {
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new HangfireDashboardAuthFilter() },
            });
            

            // RecurringJob.AddOrUpdate<ISitemapService>("ISitemapService.WriteSitemap",
            //     service => service.WriteSitemap(),
            //     Cron.Daily(1, 0));
            //
            // RecurringJob.AddOrUpdate<IPublishScheduledWebpagesTask>("IPublishScheduledWebpagesTask.Execute",
            //     service => service.Execute(),
            //     Cron.Minutely());
            //
            // RecurringJob.AddOrUpdate<IClearFormEntries>("IClearFormEntries.Execute",
            //     service => service.Execute(),
            //     Cron.Daily(1,0));
            //
            // RecurringJob.AddOrUpdate<IDeleteExpiredLogsTask>("IDeleteExpiredLogsTask.Execute",
            //     service => service.Execute(),
            //     Cron.Hourly());
            //
            // RecurringJob.AddOrUpdate<ISendQueuedMessagesTask>("ISendQueuedMessagesTask.Execute",
            //     service => service.Execute(),
            //     Cron.Minutely());
            //
            // RecurringJob.AddOrUpdate<IRefreshTextSearchIndex>("IRefreshTextSearchIndex.Refresh",
            //     service => service.Refresh(),
            //     Cron.Daily(23, 0));

        }
    }
}