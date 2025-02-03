using System;
using System.Collections.Generic;
using System.Threading;
using MrCMS.AI.Models;
using MrCMS.AI.Services.Core;

namespace MrCMS.AI.Services.Texts;

public class GenerateWebpageBlocksAiTextService(IServiceProvider serviceProvider) : BaseAiTextService(serviceProvider), IGenerateWebpageBlocksAiTextService
{
    protected override IEnumerable<string> TokenNames => new[] { "block" };

    private string _prompt;
    protected override string Prompt => _prompt;

    public async IAsyncEnumerable<TokenResponse> GenerateBlocks(string userPrompt, CancellationToken cancellationToken = default)
    {
        //Todo: Implement the logic to get the content token description
        var contentTokenDescription = string.Empty;
        
        // Construct the prompt.
        _prompt = Settings.GenerateWebpageBlocksPromptTemplate
            .Replace("{{User Prompt}}", userPrompt)
            .Replace("{{Content Token Description}}", contentTokenDescription);
        
        // Stream responses from the AI provider
        await foreach (var response in ProcessStreamAsync(cancellationToken))
        {
            yield return response;
        }
    }
}
    