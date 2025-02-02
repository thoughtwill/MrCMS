using System.Collections.Generic;
using MrCMS.AI.Models;
using MrCMS.Entities.Documents.Web;

namespace MrCMS.AI.Services;

public interface IGenerateWebpageSeoAiService
{
    IAsyncEnumerable<TokenResponse> GenerateSeo(Webpage webpage);
}