using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using MrCMS.Entities.Documents.Web;
using MrCMS.Helpers;
using MrCMS.Settings;
using MrCMS.Website.Caching;

namespace MrCMS.Website.RichSnippet;

public class GenerateRichSnippetService : IGenerateRichSnippetService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly SEOSettings _settings;
    private readonly ICacheManager _cacheManager;

    public GenerateRichSnippetService(IServiceProvider serviceProvider, SEOSettings settings,
        ICacheManager cacheManager)
    {
        _serviceProvider = serviceProvider;
        _settings = settings;
        _cacheManager = cacheManager;
    }

    public async Task<IHtmlContent> Generate<T>(T page, CancellationToken cancellationToken = default) where T : Webpage
    {
        if (!_settings.EnableRichSnippets || page == null)
        {
            return HtmlString.Empty;
        }

        return await _cacheManager.GetOrCreateAsync($"json-ld-{page.Id}", async () =>
        {
            var generatorTypes = TypeHelper.GetAllConcreteTypesAssignableFrom<IRichSnippetGenerator<T>>();

            var result = new List<string>();

            foreach (var type in generatorTypes)
            {
                if (_serviceProvider.GetService(type) is not IRichSnippetGenerator<T> generator)
                {
                    continue;
                }

                var jsonLdString = await generator.GenerateJsonLd(page, cancellationToken);

                if (string.IsNullOrWhiteSpace(jsonLdString))
                {
                    continue;
                }

                result.Add($"<script type=\"application/ld+json\">{jsonLdString}</script>");
            }
            
            return BuildHtmlContent(result);
        }, TimeSpan.FromMinutes(15), CacheExpiryType.Sliding);
    }

    private IHtmlContent BuildHtmlContent(List<string> result)
    {
        return result.Count > 0 ? new HtmlString(string.Join("\n", result)) : HtmlString.Empty;
    }
}