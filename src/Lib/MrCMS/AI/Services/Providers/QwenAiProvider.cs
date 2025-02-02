using System;
using System.Collections.Generic;
using MrCMS.AI.Models;
using MrCMS.AI.Services.Core;

namespace MrCMS.AI.Services.Providers;

public class QwenAiProvider : IAiProvider
{
    public IAsyncEnumerable<AiRawResponse> StreamResponseAsync(string prompt)
    {
        throw new NotImplementedException();
    }
}