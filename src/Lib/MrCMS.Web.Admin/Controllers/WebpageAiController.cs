using System.Collections.Generic;
using MrCMS.AI.Models;
using MrCMS.AI.Services;
using MrCMS.Web.Admin.Infrastructure.BaseControllers;
using MrCMS.Web.Admin.Services;
using MrCMS.Web.Admin.Services.Content;

namespace MrCMS.Web.Admin.Controllers;

public class WebpageAiController : MrCMSAdminController
{
    private readonly IEnhanceWebpageContentAiService _enhanceContentAiService;
    private readonly IGenerateWebpageSeoAiService _seoContentAiService;
    private readonly IGenerateWebpageBlocksAiService _generateBlocksAiService;
    private readonly IWebpageAdminService _webpageAdminService;
    private readonly IContentVersionAdminService _contentVersionAdminService;
    private readonly IContentBlockAdminService _contentBlockAdminService;

    public WebpageAiController(IEnhanceWebpageContentAiService enhanceContentAiService, IGenerateWebpageSeoAiService seoContentAiService,
        IGenerateWebpageBlocksAiService generateBlocksAiService, IWebpageAdminService webpageAdminService,
        IContentVersionAdminService contentVersionAdminService, IContentBlockAdminService contentBlockAdminService)
    {
        _enhanceContentAiService = enhanceContentAiService;
        _seoContentAiService = seoContentAiService;
        _generateBlocksAiService = generateBlocksAiService;
        _webpageAdminService = webpageAdminService;
        _contentVersionAdminService = contentVersionAdminService;
        _contentBlockAdminService = contentBlockAdminService;
    }

    public async IAsyncEnumerable<TokenResponse> EnhanceContent(int webpageId)
    {
        var webpage = await _webpageAdminService.GetWebpage(webpageId);
        if (webpage == null)
        {
            yield break;
        }

        await foreach (var rawResponse in _enhanceContentAiService.EnhanceContent(webpage))
        {
            yield return rawResponse;
        }
    }

    public async IAsyncEnumerable<TokenResponse> GenerateSeo(int webpageId)
    {
        var webpage = await _webpageAdminService.GetWebpage(webpageId);
        if (webpage == null)
        {
            yield break;
        }

        await foreach (var rawResponse in _seoContentAiService.GenerateSeo(webpage))
        {
            yield return rawResponse;
        }
    }

    public async IAsyncEnumerable<TokenResponse> GenerateContent(int contentVersionId, string userPrompt)
    {
        var contentVersion = await _contentVersionAdminService.GetVersions(contentVersionId);
        if (contentVersion == null)
        {
            yield break;
        }

        await foreach (var rawResponse in _generateBlocksAiService.GenerateBlocks(userPrompt))
        {
            yield return rawResponse;
            //TODO: Save the generated content to the content block
        }
    }
}