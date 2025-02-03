using System.Collections.Generic;
using System.Threading;
using MrCMS.AI.Models;
using MrCMS.Entities.Documents.Web;

namespace MrCMS.AI.Services.Texts;

public interface IEnhanceWebpageContentAiTextService
{
    IAsyncEnumerable<TokenResponse> EnhanceContent(Webpage webpage, CancellationToken cancellationToken = default);
}