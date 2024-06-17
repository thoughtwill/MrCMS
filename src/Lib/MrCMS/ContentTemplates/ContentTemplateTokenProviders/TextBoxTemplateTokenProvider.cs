using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using MrCMS.ContentTemplates.ContentTemplateTokenProviders.Base;
using MrCMS.ContentTemplates.Models;

namespace MrCMS.ContentTemplates.ContentTemplateTokenProviders;

public class TextBoxTemplateTokenProvider : ContentTemplateTokenProvider
{
    public override string Icon => "fa fa-file-text";
    public override string DisplayName => "Text";
    public override string HtmlPattern => $"[{Name} name=\"{Name}1\" default=\"\"]";
    
    public override async Task<IHtmlContent> ViewRenderAsync(IHtmlHelper helper, ViewRenderElementProperty property)
    {
        if (string.IsNullOrWhiteSpace(property.Value) && property.Attributes != null)
        {
            property.Value = property.Attributes.Aggregate(property.Value, (current, attr) => attr.Key switch
            {
                "default" => attr.Value,
                _ => current
            });
        }

        return await Task.FromResult(new HtmlString(property.Value));
    }
    
    public override string AdminRenderResponsiveClass => "col-md-6 col-lg-4 col-xl-3";

    public override async Task<IHtmlContent> AdminRenderAsync(IHtmlHelper helper, AdminRenderElementProperty property)
    {
        if (string.IsNullOrWhiteSpace(property.Value) && property.Attributes != null)
        {
            property.Value = property.Attributes.Aggregate(property.Value, (current, attr) => attr.Key switch
            {
                "default" => attr.Value,
                _ => current
            });
        }
            
        var tagBuilder = new TagBuilder("input")
        {
            Attributes =
            {
                ["id"] = property.Id,
                ["name"] = property.Name,
                ["type"] = "text",
                ["value"] = property.Value,
                ["data-content-template-input"] = null
            }
        };

        tagBuilder.AddCssClass("form-control");

        return await Task.FromResult(tagBuilder);
    }
}