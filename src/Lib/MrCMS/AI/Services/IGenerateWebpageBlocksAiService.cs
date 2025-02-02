using System.Collections.Generic;
using MrCMS.AI.Models;

namespace MrCMS.AI.Services;

public interface IGenerateWebpageBlocksAiService
{
    IAsyncEnumerable<TokenResponse> GenerateBlocks(string userPrompt);
}