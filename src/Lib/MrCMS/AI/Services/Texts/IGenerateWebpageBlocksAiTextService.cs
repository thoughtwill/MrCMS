using System.Collections.Generic;
using System.Threading;
using MrCMS.AI.Models;

namespace MrCMS.AI.Services.Texts;

public interface IGenerateWebpageBlocksAiTextService
{
    IAsyncEnumerable<TokenResponse> GenerateBlocks(GenerateBlocksInput input, CancellationToken cancellationToken = default);
}