using System.Collections.Generic;
using MrCMS.AI.Models;

namespace MrCMS.AI.Services.Core;

public interface IAiProvider
{
    IAsyncEnumerable<AiRawResponse> StreamResponseAsync(string prompt);
}