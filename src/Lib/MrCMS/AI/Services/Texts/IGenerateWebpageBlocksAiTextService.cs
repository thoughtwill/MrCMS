using System.Collections.Generic;
using System.Threading;
using MrCMS.AI.Models;

namespace MrCMS.AI.Services.Texts;

public interface IGenerateWebpageBlocksAiTextService
{
    IAsyncEnumerable<TokenResponse> GenerateBlocks(string userPrompt, CancellationToken cancellationToken = default);
}