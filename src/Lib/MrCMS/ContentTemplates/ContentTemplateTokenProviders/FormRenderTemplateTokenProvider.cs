using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using MrCMS.ContentTemplates.ContentTemplateTokenProviders.Base;
using MrCMS.ContentTemplates.Models;
using MrCMS.Entities.Documents.Web;
using MrCMS.Helpers;
using NHibernate;
using NHibernate.Linq;

namespace MrCMS.ContentTemplates.ContentTemplateTokenProviders;

public class FormRenderTemplateTokenProvider : ContentTemplateTokenProvider
{
    private readonly ISession _session;

    public FormRenderTemplateTokenProvider(ISession session)
    {
        _session = session;
    }
    
    public override string Icon => "fa fa-wpforms";

    public override async Task<IHtmlContent> ViewRenderAsync(IHtmlHelper helper, ViewRenderElementProperty property)
    {
        if (string.IsNullOrWhiteSpace(property.Value))
        {
            return await Task.FromResult(HtmlString.Empty);
        }

        return await Task.FromResult(helper.ParseShortcodes($"[form id=\"{property.Value}\"]"));
    }

    public override string AdminRenderResponsiveClass => "col-md-6 col-lg-4 col-xl-3";

    public override async Task<IHtmlContent> AdminRenderAsync(IHtmlHelper helper, AdminRenderElementProperty property)
    {
        var values = await _session.Query<Form>().Select(f => new { f.Id, f.Name }).ToListAsync();

        var tagBuilder = new TagBuilder("select")
        {
            Attributes =
            {
                ["id"] = property.Id,
                ["name"] = property.Name,
                ["data-content-template-input"] = null
            }
        };

        tagBuilder.AddCssClass("form-control");

        foreach (var value in values)
        {
            var option = new TagBuilder("option")
            {
                Attributes =
                {
                    ["value"] = value.Id.ToString()
                }
            };

            if (value.Id.ToString() == property.Value)
            {
                option.Attributes.Add("selected", "selected");
            }

            option.InnerHtml.Append(value.Name);

            tagBuilder.InnerHtml.AppendHtml(option);
        }

        return await Task.FromResult(tagBuilder);
    }
}