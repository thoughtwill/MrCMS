using MrCMS.Entities.Documents.Layout;
using MrCMS.Entities.Documents.Web;
using MrCMS.Settings;
using NHibernate;

namespace MrCMS.Services
{
    public class GetCurrentLayout : IGetCurrentLayout
    {
        private readonly PageDefaultsSettings _pageDefaultsSettings;
        private readonly IWebpageMetadataService _webpageMetadataService;
        private readonly ISession _session;
        private readonly SiteSettings _siteSettings;

        public GetCurrentLayout(ISession session, SiteSettings siteSettings, PageDefaultsSettings pageDefaultsSettings,
            IWebpageMetadataService webpageMetadataService)
        {
            _session = session;
            _siteSettings = siteSettings;
            _pageDefaultsSettings = pageDefaultsSettings;
            _webpageMetadataService = webpageMetadataService;
        }

        public Layout Get(Webpage webpage)
        {
            if (webpage != null)
            {
                var layout = GetPageTemplateLayout(webpage);
                if (layout != null) return layout;
                layout = GetTypeDefaultLayout(webpage);
                if (layout != null) return layout;
            }

            return GetSiteDefault();
        }

        public Layout GetUserAccountLayout()
        {
            var siteSettingsUserAccountLayoutId = _siteSettings.UserAccountLayoutId;
            return siteSettingsUserAccountLayoutId > 0 ? _session.Get<Layout>(siteSettingsUserAccountLayoutId) : GetSiteDefault();
        }

        private Layout GetTypeDefaultLayout(Webpage webpage)
        {
            var webpageMetadata = _webpageMetadataService.GetMetadata(webpage);
            var layoutId = _pageDefaultsSettings.GetLayoutId(webpageMetadata.Type);
            if (layoutId.HasValue)
            {
                var layout = _session.Get<Layout>(layoutId);
                if (layout != null) return layout;
            }

            return null;
        }

        private static Layout GetPageTemplateLayout(Webpage webpage)
        {
            return webpage.PageTemplate is { Layout: not null } ? webpage.PageTemplate.Layout : null;
        }

        public Layout GetSiteDefault()
        {
            var settingValue = _siteSettings.DefaultLayoutId;

            return _session.Get<Layout>(settingValue) ??
                   _session.QueryOver<Layout>()
                       .Take(1)
                       .Cacheable()
                       .SingleOrDefault();
        }
    }
}
