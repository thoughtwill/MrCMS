using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using MrCMS.ContentTemplates.ContentTemplateTokenProviders.Base;
using MrCMS.ContentTemplates.Models;
using MrCMS.Helpers;

namespace MrCMS.ContentTemplates.ContentTemplateTokenProviders;

public class SelectTemplateTokenProvider : ContentTemplateTokenProvider
{
    public override string Icon => "fa fa-caret-square-o-down";
    public override string DisplayName => "Dropdown";
    public override string HtmlPattern => $"[{Name} name=\"{Name}1\" values=\"Value|Text,ValueOnly\" default=\"Value\"]";
    
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

        var values = new List<string>();
        foreach (var attr in property.Attributes?.ToList() ?? new List<AttributeItem>())
        {
            switch (attr.Key)
            {
                case "values":
                    var items = attr.Value.Split(",".ToCharArray(),
                        StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    if (items.Any())
                    {
                        values.AddRange(items);
                    }

                    break;
            }
        }

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
            var tempValue = value;
            var text = value;
            if (value.Contains('|'))
            {
                tempValue = value.Split('|')[0];
                text = value.Split('|')[1];
            }

            var option = new TagBuilder("option")
            {
                Attributes =
                {
                    ["value"] = tempValue
                }
            };

            if (tempValue == property.Value)
            {
                option.Attributes.Add("selected", "selected");
            }

            option.InnerHtml.Append(text.BreakUpString());

            tagBuilder.InnerHtml.AppendHtml(option);
        }

        return await Task.FromResult(tagBuilder);
    }
}