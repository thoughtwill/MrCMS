using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using MrCMS.ContentTemplates.ContentTemplateTokenProviders.Base;
using MrCMS.ContentTemplates.Models;

namespace MrCMS.ContentTemplates.ContentTemplateTokenProviders;

public class ArrayIndexConditionTemplateTokenProvider : ContentTemplateTokenProvider
{
    public override string Icon => "fa fa-indent";
    public override string HtmlPattern => $"[{Name} name=\"{Name}1\" if=\"0\" then=\"active\"]";

    public override async Task<IHtmlContent> ViewRenderAsync(IHtmlHelper helper, ViewRenderElementProperty property)
    {
        var value = "";
        int? equalValue = null;
        if (property.Attributes != null)
        {
            foreach (var attr in property.Attributes)
            {
                switch (attr.Key)
                {
                    case "if":
                        if (int.TryParse(attr.Value, out int parsedValue))
                        {
                            equalValue = parsedValue;
                        }
                        else
                        {
                            equalValue = null;
                        }

                        break;
                    case "then":
                        value = attr.Value;
                        break;
                }
            }
        }

        if (equalValue.HasValue && property.Index.HasValue && equalValue == property.Index)
        {
            return await Task.FromResult(new HtmlString(value));
        }

        return await Task.FromResult(HtmlString.Empty);
    }
    
    public override string AdminRenderResponsiveClass => "col-12 d-none";

    public override async Task<IHtmlContent> AdminRenderAsync(IHtmlHelper helper, AdminRenderElementProperty property)
    {
        return await Task.FromResult(HtmlString.Empty);
    }
}