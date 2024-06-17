using System.Drawing;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using MrCMS.ContentTemplates.ContentTemplateTokenProviders.Base;
using MrCMS.ContentTemplates.Models;
using MrCMS.Helpers;

namespace MrCMS.ContentTemplates.ContentTemplateTokenProviders;

public class MediaSelectorTemplateTokenProvider : ContentTemplateTokenProvider
{
    public override string Name => "Media";
    public override string DisplayName => "Image";
    public override string Icon => "fa fa-image";
    public override string HtmlPattern => $"[{Name} name=\"{Name}1\" width=\"\" height=\"\" class=\"\"]";

    public override async Task<IHtmlContent> ViewRenderAsync(IHtmlHelper helper, ViewRenderElementProperty property)
    {
        var classes = (string)null;
        var alt = (string)null;
        var width = 0;
        var height = 0;
        if (property.Attributes != null)
            foreach (var attr in property.Attributes)
            {
                switch (attr.Key)
                {
                    case "width":
                        int.TryParse(attr.Value, out width);
                        break;
                    case "height":
                        int.TryParse(attr.Value, out height);
                        break;
                    case "class":
                        classes = attr.Value;
                        break;
                    case "alt":
                        alt = attr.Value;
                        break;
                }
            }

        var size = default(Size);
        if (width > 0)
            size = new Size { Width = width };

        if (height > 0)
            size.Height = height;

        return await helper.RenderImage(property.Value, size, alt, attributes: new { @class = classes },
            enableLazyLoading: false);
    }
    
    public override string AdminRenderResponsiveClass => "col-12";

    public override async Task<IHtmlContent> AdminRenderAsync(IHtmlHelper helper, AdminRenderElementProperty property)
    {
        IHtmlContentBuilder htmlContent = new HtmlContentBuilder();

        var tagBuilder = new TagBuilder("input")
        {
            Attributes =
            {
                ["type"] = "text",
                ["name"] = property.Name,
                ["value"] = property.Value,
                ["data-type"] = "media-selector",
                ["id"] = property.Id,
                ["data-content-template-input"] = null
            }
        };
        tagBuilder.AddCssClass("form-control");

        var breakTag = new TagBuilder("br")
        {
            TagRenderMode = TagRenderMode.SelfClosing
        };

        htmlContent.AppendHtml(breakTag);
        htmlContent.AppendHtml(tagBuilder);

        return await Task.FromResult(htmlContent);
    }
}