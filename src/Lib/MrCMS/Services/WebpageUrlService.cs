using System;
using System.Threading.Tasks;
using MrCMS.Entities.Documents.Web;
using MrCMS.Helpers;
using MrCMS.Models;
using MrCMS.Settings;
using NHibernate;

namespace MrCMS.Services
{
    public class WebpageUrlService : IWebpageUrlService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IEnsureWebpageUrlIsValid _ensureWebpageUrlIsValid;
        private readonly ISession _session;
        private readonly PageDefaultsSettings _settings;

        public WebpageUrlService(IEnsureWebpageUrlIsValid ensureWebpageUrlIsValid, ISession session,
            IServiceProvider serviceProvider,
            PageDefaultsSettings settings)
        {
            _ensureWebpageUrlIsValid = ensureWebpageUrlIsValid;
            _session = session;
            _serviceProvider = serviceProvider;
            _settings = settings;
        }

        public async Task<string> Suggest(int siteId, SuggestParams suggestParams)
        {
            var webpageType = suggestParams.WebpageType;
            var parts = webpageType.Split(new[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2)
            {
                suggestParams.WebpageType = parts[0];
                suggestParams.Template = int.TryParse(parts[1], out var id) ? id : (int?)null;
            }

            IWebpageUrlGenerator generator = await GetGenerator(suggestParams.WebpageType, suggestParams.Template);
            var parent = suggestParams.ParentId.HasValue ? await _session.GetAsync<Webpage>(suggestParams.ParentId.Value) : null;

            string url = generator.GetUrl(suggestParams.PageName, parent, suggestParams.UseHierarchy);

            //make sure the URL is unique
            return await _ensureWebpageUrlIsValid.GetValidUrl(siteId, url, suggestParams.WebpageId);
        }

        private async Task<IWebpageUrlGenerator> GetGenerator(string webpageType, int? template)
        {
            IWebpageUrlGenerator generator = null;
            int id = template.GetValueOrDefault(0);
            if (id > 0)
            {
                var pageTemplate = await _session.GetAsync<PageTemplate>(id);
                Type urlGeneratorType = GetUrlGeneratorType(pageTemplate);
                if (pageTemplate != null && urlGeneratorType != null)
                {
                    generator = _serviceProvider.GetService(urlGeneratorType) as IWebpageUrlGenerator;
                }
            }

            if (generator == null && webpageType != null)
            {
                generator =
                    _serviceProvider.GetService(_settings.GetGeneratorType(webpageType)) as IWebpageUrlGenerator;
            }

            return generator ?? new DefaultWebpageUrlGenerator();
        }

        private static Type GetUrlGeneratorType(PageTemplate template)
        {
            return template != null
                ? TypeHelper.GetTypeByName(template.UrlGeneratorType)
                : null;
        }
    }
}