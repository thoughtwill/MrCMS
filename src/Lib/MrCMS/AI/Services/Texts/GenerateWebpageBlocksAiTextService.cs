using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using MrCMS.AI.Models;
using MrCMS.AI.Services.Core;
using MrCMS.Entities.Documents.Web;
using NHibernate;

namespace MrCMS.AI.Services.Texts;

public class GenerateWebpageBlocksAiTextService(IServiceProvider serviceProvider, ISession session) : BaseAiTextService(serviceProvider), IGenerateWebpageBlocksAiTextService
{
    protected override IEnumerable<string> TokenNames => new[] { "block" };

    private string _prompt;
    protected override string Prompt => _prompt;

    public async IAsyncEnumerable<TokenResponse> GenerateBlocks(GenerateBlocksInput input, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var contentVersion = await session.GetAsync<ContentVersion>(input.ContentVersionId, cancellationToken);
        if (contentVersion == null)
        {
            yield break;
        }
        
        //Todo: Implement the logic to get the content token description
        var contentTokenDescription = string.Empty;
        
        // Construct the prompt.
        _prompt = Settings.GenerateWebpageBlocksPromptTemplate
            .Replace("{{User Prompt}}", input.Prompt)
            .Replace("{{Content Token Description}}", contentTokenDescription);
        
        // Stream responses from the AI provider
        await foreach (var response in ProcessStreamAsync(cancellationToken))
        {
            yield return response;
        }
    }
}
    