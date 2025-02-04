using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using MrCMS.AI.Models;
using MrCMS.AI.Services.Core;
using MrCMS.Entities.Documents.Web;
using NHibernate;

namespace MrCMS.AI.Services.Texts;

public class EnhanceWebpageContentAiTextService(IServiceProvider serviceProvider, ISession session)
    : BaseAiTextService(serviceProvider), IEnhanceWebpageContentAiTextService
{
    protected override IEnumerable<string> TokenNames => new[] { "title", "content" };

    private string _prompt;
    protected override string Prompt => _prompt;

    public async IAsyncEnumerable<TokenResponse> EnhanceContent(EnhanceContentInput input,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var webpage = await session.GetAsync<Webpage>(input.WebpageId, cancellationToken);
        if (webpage == null)
        {
            yield break;
        }

        // Construct the prompt.
        _prompt = Settings.EnhanceWebpageContentPromptTemplate
            .Replace("{{Title}}", webpage.Name)
            .Replace("{{Body Content}}", webpage.BodyContent)
            .Replace("{{User Prompt}}", input.Prompt);

        // Stream responses from the AI provider
        await foreach (var response in ProcessStreamAsync(cancellationToken))
        {
            yield return response;
        }
    }
}