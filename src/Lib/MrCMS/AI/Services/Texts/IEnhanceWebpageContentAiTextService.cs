using System.Collections.Generic;
using System.Threading;
using MrCMS.AI.Models;

namespace MrCMS.AI.Services.Texts;

public interface IEnhanceWebpageContentAiTextService
{
    IAsyncEnumerable<TokenResponse> EnhanceContent(EnhanceContentInput input, CancellationToken cancellationToken = default);
}