using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using MrCMS.ContentTemplates.ContentTemplateTokenProviders.Base;
using MrCMS.ContentTemplates.Models;

namespace MrCMS.ContentTemplates.ContentTemplateTokenProviders;

public class UniqueKeyTemplateTokenProvider : ContentTemplateTokenProvider
{
    public override string Icon => "fa fa-id-badge";
    public override string DisplayName => "Unique Id";
    
    public override async Task<IHtmlContent> ViewRenderAsync(IHtmlHelper helper, ViewRenderElementProperty property)
    {
        return await Task.FromResult(new HtmlString(property.Value));
    }
    
    public override string AdminRenderResponsiveClass => "col-12 d-none";
    
    public override bool GlobalRender => true;

    public override async Task<IHtmlContent> AdminRenderAsync(IHtmlHelper helper, AdminRenderElementProperty property)
    {
        IHtmlContentBuilder htmlContent = new HtmlContentBuilder();

        var tagBuilder = new TagBuilder("input")
        {
            Attributes =
            {
                ["type"] = "hidden",
                ["name"] = property.Name,
                ["value"] = property.Id,
                ["data-unique-key"] = null,
                ["data-content-template-input"] = null
            }
        };

        tagBuilder.Attributes["id"] = property.Id;

        htmlContent.AppendHtml(tagBuilder);

        var label = new TagBuilder("span");
        label.InnerHtml.Append(property.Id);
        label.AddCssClass("fw-bold");

        htmlContent.AppendHtml(label);

        return await Task.FromResult(htmlContent);
    }
}