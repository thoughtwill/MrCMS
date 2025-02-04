using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using MrCMS.AI.Models;
using MrCMS.AI.Services.Texts;
using MrCMS.Web.Admin.Infrastructure.BaseControllers;
using MrCMS.Web.Admin.Services;

namespace MrCMS.Web.Admin.Controllers;

public class WebpageAiController : MrCMSAdminController
{
    private readonly IEnhanceWebpageContentAiTextService _enhanceContentAiTextService;
    private readonly IGenerateWebpageSeoAiTextService _seoContentAiTextService;
    private readonly IGenerateWebpageBlocksAiTextService _generateBlocksAiTextService;

    public WebpageAiController(IEnhanceWebpageContentAiTextService enhanceContentAiTextService,
        IGenerateWebpageSeoAiTextService seoContentAiTextService,
        IGenerateWebpageBlocksAiTextService generateBlocksAiTextService)
    {
        _enhanceContentAiTextService = enhanceContentAiTextService;
        _seoContentAiTextService = seoContentAiTextService;
        _generateBlocksAiTextService = generateBlocksAiTextService;
    }

    [HttpGet]
    public IActionResult EnhanceContent(int webpageId)
    {
        return View(new EnhanceContentInput { WebpageId = webpageId });
    }


    [HttpPost]
    public async IAsyncEnumerable<TokenResponse> EnhanceContent([FromBody] EnhanceContentInput input,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var rawResponse in _enhanceContentAiTextService.EnhanceContent(input, cancellationToken))
        {
            yield return rawResponse;
        }
    }
    
    [HttpGet]
    public IActionResult GenerateSeo(int webpageId)
    {
        return View(new GenerateSeoInput { WebpageId = webpageId });
    }

    [HttpPost]
    public async IAsyncEnumerable<TokenResponse> GenerateSeo([FromBody] GenerateSeoInput input,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var rawResponse in _seoContentAiTextService.GenerateSeo(input, cancellationToken))
        {
            yield return rawResponse;
        }
    }
    
    [HttpGet]
    public IActionResult GenerateContent(int contentVersionIdId)
    {
        return View(new GenerateBlocksInput { ContentVersionId = contentVersionIdId });
    }

    [HttpPost]
    public async IAsyncEnumerable<TokenResponse> GenerateContent([FromBody] GenerateBlocksInput input,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var rawResponse in _generateBlocksAiTextService.GenerateBlocks(input, cancellationToken))
        {
            yield return rawResponse;
            //TODO: Save the generated content to the content block
        }
    }
}