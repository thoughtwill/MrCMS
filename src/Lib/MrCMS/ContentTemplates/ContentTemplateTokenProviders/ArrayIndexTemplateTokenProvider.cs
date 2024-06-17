using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using MrCMS.ContentTemplates.ContentTemplateTokenProviders.Base;
using MrCMS.ContentTemplates.Models;

namespace MrCMS.ContentTemplates.ContentTemplateTokenProviders;

public class ArrayIndexTemplateTokenProvider : ContentTemplateTokenProvider
{
    public override string Icon => "fa fa-indent";

    public override async Task<IHtmlContent> ViewRenderAsync(IHtmlHelper helper, ViewRenderElementProperty property)
    {
        return await Task.FromResult(property.Index.HasValue ? new HtmlString(property.Index.Value.ToString()) : HtmlString.Empty);
    }
    
    public override string AdminRenderResponsiveClass => "col-12 d-none";

    public override async Task<IHtmlContent> AdminRenderAsync(IHtmlHelper helper, AdminRenderElementProperty property)
    {
        return await Task.FromResult(HtmlString.Empty);
    }
}