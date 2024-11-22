using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MrCMS.Entities.Documents.Web;

namespace MrCMS.Website.RichSnippet;

public interface IRichSnippetGenerator<in T> where T : Webpage
{
    [ItemCanBeNull] Task<string> GenerateJsonLd(T page, CancellationToken cancellationToken = default);
}
