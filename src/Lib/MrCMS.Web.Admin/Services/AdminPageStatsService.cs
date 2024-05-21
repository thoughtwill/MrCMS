using System.Collections.Generic;
using MrCMS.Entities.Documents.Web;
using MrCMS.Services;
using MrCMS.Web.Admin.Models;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;

namespace MrCMS.Web.Admin.Services
{
    public class AdminPageStatsService : IAdminPageStatsService
    {
        private readonly ISession _session;
        private readonly ICurrentSiteLocator _siteLocator;

        public AdminPageStatsService(ISession session, ICurrentSiteLocator siteLocator)
        {
            _session = session;
            _siteLocator = siteLocator;
        }

        public IList<WebpageStats> GetSummary()
        {
            WebpageStats countAlias = null;
            Webpage webpageAlias = null;
            var site = _siteLocator.GetCurrentSite();
            return _session.QueryOver(() => webpageAlias)
                .Where(x => x.Site.Id == site.Id)
                .SelectList(
                    builder =>
                        builder.SelectGroup(() => webpageAlias.WebpageTypeName)
                            .WithAlias(() => countAlias.WebpageType)
                            .SelectCount(() => webpageAlias.Id)
                            .WithAlias(() => countAlias.NumberOfPages)
                            .SelectSubQuery(
                                QueryOver.Of<Webpage>()
                                    .Where(
                                        webpage =>
                                            webpage.Site.Id == site.Id &&
                                            webpage.WebpageTypeName == webpageAlias.WebpageTypeName &&
                                           !webpage.Published && !webpage.IsDeleted)
                                    .ToRowCountQuery())
                            .WithAlias(() => countAlias.NumberOfUnPublishedPages))
                .TransformUsing(Transformers.AliasToBean<WebpageStats>())
                .Cacheable()
                .List<WebpageStats>();
        }
    }
}