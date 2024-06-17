using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;

namespace MrCMS.ContentTemplates.Core;

public interface IContentTemplateViewRenderer
{
    Task<IHtmlContent> ParseAsync(IHtmlHelper helper, string text, string props);
}