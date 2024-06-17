using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using MrCMS.ContentTemplates.ContentTemplateTokenProviders.Base;
using MrCMS.ContentTemplates.Models;
using MrCMS.Entities.Documents.Web;
using MrCMS.Services;
using NHibernate;
using NHibernate.Linq;

namespace MrCMS.ContentTemplates.ContentTemplateTokenProviders;

public class PageAnchorTagTemplateTokenProvider : ContentTemplateTokenProvider
{
    private readonly IWebpageUIService _webpageUiService;
    private readonly IGetHomePage _getHomePage;
    private readonly ISession _session;

    public PageAnchorTagTemplateTokenProvider(IWebpageUIService webpageUiService, IGetHomePage getHomePage, ISession session)
    {
        _webpageUiService = webpageUiService;
        _getHomePage = getHomePage;
        _session = session;
    }
    
    public override string Icon => "fa fa-anchor";
    public override string HtmlPattern => $"[{Name} name=\"{Name}1\" class=\"\"]";

    public override async Task<IHtmlContent> ViewRenderAsync(IHtmlHelper helper, ViewRenderElementProperty property)
    {
        if (int.TryParse(property.Value, out var pageId))
        {
            var page  = await _webpageUiService.GetPage<Webpage>(pageId);
            
            var homePage= await _getHomePage.Get();
            
            var anchorTag = new TagBuilder("a");

            anchorTag.Attributes.Add("href", homePage?.Id == page.Id ? "/" : $"/{page?.UrlSegment}");

            anchorTag.InnerHtml.Append(page?.Name ?? string.Empty);

            if (property.Attributes != null)
            {
                foreach (var attr in property.Attributes)
                {
                    anchorTag.Attributes.Add(attr.Key,attr.Value);
                }
            }

            return anchorTag;
        }
        
        return new HtmlString(property.Value);
    }
    
    public override string AdminRenderResponsiveClass => "col-md-6 col-lg-4 col-xl-3";

    public override async Task<IHtmlContent> AdminRenderAsync(IHtmlHelper helper, AdminRenderElementProperty property)
    {
        var tagBuilder = new TagBuilder("select")
        {
            Attributes =
            {
                ["id"] = property.Id,
                ["name"] = property.Name,
                ["data-webpage-url-selector"] = null,
                ["data-content-template-input"] = null
            }
        };

        Webpage page = null;

        if (string.IsNullOrEmpty(property.Value))
        {
            page = await _session.Query<Webpage>()
                .FirstOrDefaultAsync(x => x.WebpageType.EndsWith(property.Name));
        }
        else if (int.TryParse(property.Value, out var pageId))
        {
            page = await _webpageUiService.GetPage<Webpage>(pageId);
        }

        if (page != null)
        {
            tagBuilder.InnerHtml.AppendHtml($"<option value='{page.Id}' selected>{page.Name}</option>");
        }

        tagBuilder.AddCssClass("form-control");

        return await Task.FromResult(tagBuilder);
    }
}