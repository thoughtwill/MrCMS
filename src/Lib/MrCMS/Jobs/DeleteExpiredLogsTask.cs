using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using MrCMS.Entities.Multisite;
using MrCMS.Helpers;
using MrCMS.Logging;
using MrCMS.Settings;
using MrCMS.Website;
using NHibernate;
using NHibernate.Linq;

namespace MrCMS.Jobs
{
    public class DeleteExpiredLogsTask : IDeleteExpiredLogsTask
    {
        private readonly IStatelessSession _statelessSession;
        private readonly IConfigurationProviderFactory _configurationProviderFactory;
        private readonly IGetDateTimeNow _getDateTimeNow;

        public DeleteExpiredLogsTask(IStatelessSession statelessSession,
            IConfigurationProviderFactory configurationProviderFactory, IGetDateTimeNow getDateTimeNow)
        {
            _statelessSession = statelessSession;
            _configurationProviderFactory = configurationProviderFactory;
            _getDateTimeNow = getDateTimeNow;
        }

        [DisableConcurrentExecution(timeoutInSeconds: 3600)]
        public async Task Execute()
        {
            foreach (var site in await _statelessSession.Query<Site>().ToListAsync())
            {
                var configProvider = _configurationProviderFactory.GetForSite(site);
                var siteSettings = configProvider.GetSiteSettings<SiteSettings>();
                var now = _getDateTimeNow.LocalNow;
                var expiredLogTime = now.AddDays(-siteSettings.DaysToKeepLogs);

                await _statelessSession.TransactAsync(async session =>
                {
                    await _statelessSession.Query<Log>().Where(data =>
                        data.Site.Id == site.Id && data.CreatedOn <= expiredLogTime).DeleteAsync();
                });
            }
        }
    }

    public interface IDeleteExpiredLogsTask
    {
        Task Execute();
    }
}