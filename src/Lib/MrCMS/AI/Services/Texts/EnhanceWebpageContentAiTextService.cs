using System;
using System.Collections.Generic;
using System.Threading;
using MrCMS.AI.Models;
using MrCMS.AI.Services.Core;
using MrCMS.Entities.Documents.Web;

namespace MrCMS.AI.Services.Texts;

public class EnhanceWebpageContentAiTextService(IServiceProvider serviceProvider) : BaseAiTextService(serviceProvider), IEnhanceWebpageContentAiTextService
{
    protected override IEnumerable<string> TokenNames => new[] { "title", "content" };

    private string _prompt;
    protected override string Prompt => _prompt;

    public async IAsyncEnumerable<TokenResponse> EnhanceContent(Webpage webpage, CancellationToken cancellationToken = default)
    {
        // Construct the prompt.
        _prompt = Settings.EnhanceWebpageContentPromptTemplate
            .Replace("{{Title}}", webpage.Name)
            .Replace("{{Body Content}}", webpage.BodyContent);
        
        // Stream responses from the AI provider
        await foreach (var response in ProcessStreamAsync(cancellationToken))
        {
            yield return response;
        }
    }
}