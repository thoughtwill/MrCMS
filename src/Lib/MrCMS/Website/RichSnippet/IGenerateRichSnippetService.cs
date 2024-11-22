using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using MrCMS.Entities.Documents.Web;

namespace MrCMS.Website.RichSnippet;

public interface IGenerateRichSnippetService
{
    Task<IHtmlContent> Generate<T>(T page, CancellationToken cancellationToken = default) where T : Webpage;
}