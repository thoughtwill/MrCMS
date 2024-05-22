using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using MrCMS.Entities.Documents.Web;
using MrCMS.Helpers;
using MrCMS.Website;
using MrCMS.Website.Caching;
using NHibernate;

namespace MrCMS.Tasks
{
    public class PublishScheduledWebpagesTask : IPublishScheduledWebpagesTask
    {
        private readonly IStatelessSession _session;
        private readonly IGetDateTimeNow _getDateTimeNow;
        private readonly ICacheManager _cacheManager;

        public PublishScheduledWebpagesTask(IStatelessSession session, IGetDateTimeNow getDateTimeNow,
            ICacheManager cacheManager)
        {
            _session = session;
            _getDateTimeNow = getDateTimeNow;
            _cacheManager = cacheManager;
        }

        [DisableConcurrentExecution(timeoutInSeconds: 3600)]
        public async Task Execute()
        {
            var now = _getDateTimeNow.LocalNow;
            var due = await _session.QueryOver<Webpage>().Where(x => !x.Published && x.PublishOn <= now).ListAsync();
            if (!due.Any())
                return;
            await _session.TransactAsync(async session =>
            {
                foreach (var webpage in due)
                {
                    webpage.Published = true;
                    await session.UpdateAsync(webpage);
                }
            });
            _cacheManager.Clear();
        }
    }

    public interface IPublishScheduledWebpagesTask
    {
        Task Execute();
    }
}