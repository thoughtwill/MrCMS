using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using MrCMS.AI.Models;

namespace MrCMS.AI.Services.Core;

public interface IAiTextProvider
{
    IAsyncEnumerable<AiTextRawResponse> StreamResponseAsync(string prompt, CancellationToken cancellationToken = default);
}