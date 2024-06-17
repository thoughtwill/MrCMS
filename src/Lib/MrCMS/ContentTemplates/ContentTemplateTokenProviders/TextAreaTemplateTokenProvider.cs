using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using MrCMS.ContentTemplates.ContentTemplateTokenProviders.Base;
using MrCMS.ContentTemplates.Models;

namespace MrCMS.ContentTemplates.ContentTemplateTokenProviders;

public class TextAreaTemplateTokenProvider : ContentTemplateTokenProvider
{
    public override string Icon => "fa fa-file-text";
    
    public override async Task<IHtmlContent> ViewRenderAsync(IHtmlHelper helper, ViewRenderElementProperty property)
    {
        return await Task.FromResult(new HtmlString(property.Value));
    }
    
    public override string AdminRenderResponsiveClass => "col-12";

    public override async Task<IHtmlContent> AdminRenderAsync(IHtmlHelper helper, AdminRenderElementProperty property)
    {
        var tagBuilder = new TagBuilder("textarea")
        {
            TagRenderMode = TagRenderMode.Normal,
            Attributes =
            {
                ["name"] = property.Name,
                ["data-content-template-input"] = null
            }
        };
        tagBuilder.InnerHtml.Append(property.Value);

        tagBuilder.Attributes["id"] = property.Id;

        tagBuilder.AddCssClass("enable-editor");
        return await Task.FromResult(tagBuilder);
    }
}