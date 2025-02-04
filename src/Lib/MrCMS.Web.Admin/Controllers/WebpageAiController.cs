using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
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

    public WebpageAiController(IEnhanceWebpageContentAiTextService enhanceContentAiTextService, IGenerateWebpageSeoAiTextService seoContentAiTextService,
        IGenerateWebpageBlocksAiTextService generateBlocksAiTextService)
    {
        _enhanceContentAiTextService = enhanceContentAiTextService;
        _seoContentAiTextService = seoContentAiTextService;
        _generateBlocksAiTextService = generateBlocksAiTextService;
    }

    public async IAsyncEnumerable<TokenResponse> EnhanceContent(EnhanceContentInput input, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var rawResponse in _enhanceContentAiTextService.EnhanceContent(input, cancellationToken))
        {
            yield return rawResponse;
        }
    }

    public async IAsyncEnumerable<TokenResponse> GenerateSeo(GenerateSeoInput input, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var rawResponse in _seoContentAiTextService.GenerateSeo(input, cancellationToken))
        {
            yield return rawResponse;
        }
    }

    public async IAsyncEnumerable<TokenResponse> GenerateContent(GenerateBlocksInput input, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var rawResponse in _generateBlocksAiTextService.GenerateBlocks(input, cancellationToken))
        {
            yield return rawResponse;
            //TODO: Save the generated content to the content block
        }
    }
}