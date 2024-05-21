using System.Linq;
using System.Threading.Tasks;
using MrCMS.DbConfiguration;
using MrCMS.Entities.Documents.Layout;
using MrCMS.Entities.Documents.Media;
using MrCMS.Entities.Documents.Web;
using MrCMS.Helpers;
using NHibernate;
using NHibernate.Linq;

namespace MrCMS.Services
{
    public class UrlValidationService : IUrlValidationService
    {
        private readonly ISession _session;

        public UrlValidationService(ISession session)
        {
            _session = session;
        }

        public async Task<bool> UrlIsValidForWebpage(int siteId, string url, int? id)
        {
            // site agnostic because we're passing in site
            using var siteFilterDisabler = new SiteFilterDisabler(_session);

            if (string.IsNullOrEmpty(url))
                return false;

            if (id.HasValue)
            {
                var webpage = await _session.GetAsync<Webpage>(id.Value);
                var documentHistory = webpage.Urls.Any(x => x.UrlSegment == url);
                if (url.Trim() == webpage.UrlSegment?.Trim() ||
                    documentHistory) //if url is the same or has been used for the same page before lets go
                    return true;

                //return !await WebpageExists(siteId, url) && !await ExistsInUrlHistory(siteId, url);
            }

            return !await WebpageExists(siteId, url) && !await ExistsInUrlHistory(siteId, url);
        }

        /// <summary>
        /// Check to see if the supplied URL is ok to be added to the URL history table
        /// </summary>
        public async Task<bool> UrlIsValidForWebpageUrlHistory(int siteId, string url)
        {
            // site agnostic because we're passing in site
            using var siteFilterDisabler = new SiteFilterDisabler(_session);

            if (string.IsNullOrEmpty(url))
                return false;

            return !await WebpageExists(siteId, url) && !await ExistsInUrlHistory(siteId, url);
        }

        public async Task<UrlHistory> Get(int id)
        {
            return await _session.GetAsync<UrlHistory>(id);
        }

        public async Task<bool> UrlIsValidForMediaCategory(int siteId, string url, int? id)
        {
            // site agnostic because we're passing in site
            using var siteFilterDisabler = new SiteFilterDisabler(_session);

            if (string.IsNullOrEmpty(url))
                return false;

            if (id.HasValue)
            {
                var mediaCategory = await _session.GetAsync<MediaCategory>(id.Value);
                if (url.Trim() == mediaCategory.Path.Trim())
                    return true;
                return !await MediaCategoryExists(siteId, url);
            }

            return !await MediaCategoryExists(siteId, url);
        }

        public async Task<bool> UrlIsValidForLayout(int siteId, string url, int? id)
        {
            // site agnostic because we're passing in site
            using var siteFilterDisabler = new SiteFilterDisabler(_session);

            if (string.IsNullOrEmpty(url))
                return false;

            if (id.HasValue)
            {
                var layout = await _session.GetAsync<Layout>(id.Value);
                if (url.Trim() == layout.Path.Trim())
                    return true;
                return !await LayoutExists(siteId, url);
            }

            return !await LayoutExists(siteId, url);
        }


        /// <summary>
        /// Checks to see if a webpage exists with the supplied URL
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="url"></param>
        /// <returns>bool</returns>
        private async Task<bool> WebpageExists(int siteId, string url)
        {
            return await _session.Query<Webpage>()
                .Where(webpage => webpage.Site.Id == siteId && webpage.UrlSegment == url)
                .WithOptions(f=>f.SetCacheable(true))
                .AnyAsync();
        }

        /// <summary>
        /// Checks to see if the supplied URL exists in webpage URL history table
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="url"></param>
        /// <returns>bool</returns>
        private Task<bool> ExistsInUrlHistory(int siteId, string url)
        {
            return
                _session.Query<UrlHistory>()
                    .Where(history => history.Site.Id == siteId && history.UrlSegment == url)
                    .WithOptions(f=> f.SetCacheable(true))
                    .AnyAsync();
        }

        /// <summary>
        /// Checks to see if the supplied url is unique for media category / folder.
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="url"></param>
        /// <returns>bool</returns>
        private Task<bool> MediaCategoryExists(int siteId, string url)
        {
            return _session.QueryOver<MediaCategory>()
                .Where(x => x.Site.Id == siteId)
                .And(doc => doc.Path == url)
                .Cacheable()
                .AnyAsync();
        }

        /// <summary>
        /// Checks to see if the supplied url is unique for layouts
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        private Task<bool> LayoutExists(int siteId, string url)
        {
            return _session.QueryOver<Layout>()
                .Where(x => x.Site.Id == siteId)
                .And(doc => doc.Path == url)
                .Cacheable()
                .AnyAsync();
        }
    }
}