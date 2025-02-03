using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MrCMS.AI.Models;
using MrCMS.AI.Services.Core;

namespace MrCMS.AI.Services.Images;

public class GenerateImageAiService
{
    private readonly IAiImageProvider _provider;

    public GenerateImageAiService(IAiImageProviderFactory aiImageProviderFactory)
    {
        _provider = aiImageProviderFactory.GetProvider();
    }
    
    public async Task<IList<AiImageResponse>> GenerateImageAsync(string prompt, CancellationToken cancellationToken = default)
    {
        var response = await _provider.GenerateImageAsync(prompt, cancellationToken);
        return response;
    }
}