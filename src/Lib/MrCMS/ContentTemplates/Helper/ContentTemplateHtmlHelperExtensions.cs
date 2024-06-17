using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading.Tasks;
using MrCMS.ContentTemplates.Core;

namespace MrCMS.ContentTemplates.Helper;

public static class ContentTemplateHtmlHelperExtensions
{
    public static async Task<IHtmlContent> RenderContentTemplate<T>(this IHtmlHelper<T> helper,
        Expression<Func<T, string>> textMethod, Expression<Func<T, string>> propertiesMethod)
    {
        var model = helper.ViewData.Model;
        if (model == null)
            return HtmlString.Empty;

        var text = textMethod?.Compile().Invoke(model);
        var props = propertiesMethod?.Compile().Invoke(model);

        return await helper.ViewContext.HttpContext.RequestServices.GetRequiredService<IContentTemplateViewRenderer>()
            .ParseAsync(helper, text, props);
    }


    public static async Task<IHtmlContent> RenderAdminProperties<T>(this IHtmlHelper<T> helper,
        Expression<Func<T, string>> textMethod, Expression<Func<T, string>> propertiesMethod)
    {
        var model = helper.ViewData.Model;
        if (model == null)
            return HtmlString.Empty;

        var text = textMethod?.Compile().Invoke(model);
        var json = propertiesMethod?.Compile().Invoke(model);
        var props = JsonSerializer.Deserialize<JsonElement>(json ?? "null");

        return await helper.ViewContext.HttpContext.RequestServices.GetRequiredService<IContentTemplateAdminRenderer>()
            .RenderAsync(helper, text, props);
    }
}