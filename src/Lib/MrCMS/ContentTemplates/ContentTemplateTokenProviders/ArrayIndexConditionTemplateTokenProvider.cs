using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using MrCMS.ContentTemplates.ContentTemplateTokenProviders.Base;
using MrCMS.ContentTemplates.Models;
using org.mariuszgromada.math.mxparser;

namespace MrCMS.ContentTemplates.ContentTemplateTokenProviders;

public class ArrayIndexConditionTemplateTokenProvider : ContentTemplateTokenProvider
{
    public override string Icon => "fa fa-indent";
    public override string HtmlPattern => $"[{Name} name=\"{Name}1\" if=\"index == 0\" then=\"active\"]";

    public override async Task<IHtmlContent> ViewRenderAsync(IHtmlHelper helper, ViewRenderElementProperty property)
    {
        var value = "";
        var result = false;
        if (property.Attributes != null)
        {
            foreach (var attr in property.Attributes)
            {
                switch (attr.Key)
                {
                    case "if":
                        string condition = attr.Value;
                        Expression expression = new Expression(condition);
                        
                        Argument index = new Argument($"index = {property.Index ?? 0}");
                        expression.addArguments(index);
                        
                        result = expression.calculate() != 0;
                        break;
                    case "then":
                        value = attr.Value;
                        break;
                }
            }
        }

        if (result)
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