using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;
using System.Threading.Tasks;

namespace MrCMS.ContentTemplates.Core;

public interface IContentTemplateAdminRenderer
{
    Task<IHtmlContent> RenderAsync(IHtmlHelper helper, string text, JsonElement properties);
}