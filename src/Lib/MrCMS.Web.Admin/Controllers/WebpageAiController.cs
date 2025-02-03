using System.Collections.Generic;
using System.Threading;
using MrCMS.AI.Models;
using MrCMS.AI.Services;
using MrCMS.AI.Services.Texts;
using MrCMS.Web.Admin.Infrastructure.BaseControllers;
using MrCMS.Web.Admin.Services;
using MrCMS.Web.Admin.Services.Content;

namespace MrCMS.Web.Admin.Controllers;

public class WebpageAiController : MrCMSAdminController
{
    private readonly IEnhanceWebpageContentAiTextService _enhanceContentAiTextService;
    private readonly IGenerateWebpageSeoAiTextService _seoContentAiTextService;
    private readonly IGenerateWebpageBlocksAiTextService _generateBlocksAiTextService;
    private readonly IWebpageAdminService _webpageAdminService;
    private readonly IContentVersionAdminService _contentVersionAdminService;

    public WebpageAiController(IEnhanceWebpageContentAiTextService enhanceContentAiTextService, IGenerateWebpageSeoAiTextService seoContentAiTextService,
        IGenerateWebpageBlocksAiTextService generateBlocksAiTextService, IWebpageAdminService webpageAdminService,
        IContentVersionAdminService contentVersionAdminService)
    {
        _enhanceContentAiTextService = enhanceContentAiTextService;
        _seoContentAiTextService = seoContentAiTextService;
        _generateBlocksAiTextService = generateBlocksAiTextService;
        _webpageAdminService = webpageAdminService;
        _contentVersionAdminService = contentVersionAdminService;
    }

    public async IAsyncEnumerable<TokenResponse> EnhanceContent(int webpageId, CancellationToken cancellationToken = default)
    {
        var webpage = await _webpageAdminService.GetWebpage(webpageId);
        if (webpage == null)
        {
            yield break;
        }

        await foreach (var rawResponse in _enhanceContentAiTextService.EnhanceContent(webpage, cancellationToken))
        {
            yield return rawResponse;
        }
    }

    public async IAsyncEnumerable<TokenResponse> GenerateSeo(int webpageId, CancellationToken cancellationToken = default)
    {
        var webpage = await _webpageAdminService.GetWebpage(webpageId);
        if (webpage == null)
        {
            yield break;
        }

        await foreach (var rawResponse in _seoContentAiTextService.GenerateSeo(webpage, cancellationToken))
        {
            yield return rawResponse;
        }
    }

    public async IAsyncEnumerable<TokenResponse> GenerateContent(int contentVersionId, string userPrompt, CancellationToken cancellationToken = default)
    {
        var contentVersion = await _contentVersionAdminService.GetVersions(contentVersionId);
        if (contentVersion == null)
        {
            yield break;
        }

        await foreach (var rawResponse in _generateBlocksAiTextService.GenerateBlocks(userPrompt, cancellationToken))
        {
            yield return rawResponse;
            //TODO: Save the generated content to the content block
        }
    }
}