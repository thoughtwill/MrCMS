using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using MrCMS.ContentTemplates.Models;
using MrCMS.Helpers;

namespace MrCMS.ContentTemplates.ContentTemplateTokenProviders.Base;

public abstract class ContentTemplateTokenProvider
{
    private string TypeName => GetType().Name.Replace("TemplateTokenProvider", "");
    public virtual string Name => TypeName.RemoveInvalidUrlCharacters();

    public virtual string AdminRenderResponsiveClass => "col-12";
    
    public virtual string Icon => "fa fa-cog";
    public virtual string HtmlPattern => $"[{Name} name=\"{Name}1\"]";
    public virtual string DisplayName => TypeName.BreakUpString();

    /// <summary>
    /// This is used for ignore array naming in admin render
    /// </summary>
    public virtual bool GlobalRender => false;

    public virtual async Task<IHtmlContent> AdminRenderAsync(IHtmlHelper helper, AdminRenderElementProperty property)
    {
        var htmlString = new HtmlString($"There is no render for {Name} yet.");
        return await Task.FromResult(htmlString);
    }

    public virtual async Task<IHtmlContent> ViewRenderAsync(IHtmlHelper helper, ViewRenderElementProperty property)
    {
        var htmlString = new HtmlString(property.Value);
        return await Task.FromResult(htmlString);
    }
}