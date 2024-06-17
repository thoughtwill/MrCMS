using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using MrCMS.ContentTemplates.ContentTemplateTokenProviders.Base;
using MrCMS.ContentTemplates.Models;
using MrCMS.Services.Resources;

namespace MrCMS.ContentTemplates.ContentTemplateTokenProviders;

public class ResourceTemplateTokenProvider : ContentTemplateTokenProvider
{
    private readonly IStringResourceProvider _stringResourceProvider;

    public ResourceTemplateTokenProvider(IStringResourceProvider stringResourceProvider)
    {
        _stringResourceProvider = stringResourceProvider;
    }
    
    public override string Icon => "fa fa-language";
    public override string DisplayName => "String Resource";
    public override string HtmlPattern => $"[{Name} name=\"key\" default=\"defaultValue\"]";
    
    public override async Task<IHtmlContent> ViewRenderAsync(IHtmlHelper helper, ViewRenderElementProperty property)
    {
        var defaultValue = property.Attributes?.FirstOrDefault(x => x.Key == "default");
        return new HtmlString(await _stringResourceProvider.GetValue(property.Name,
            options => options.SetDefaultValue(defaultValue?.Value)));
    }

    public override string AdminRenderResponsiveClass => "col-12 d-none";

    public override async Task<IHtmlContent> AdminRenderAsync(IHtmlHelper helper, AdminRenderElementProperty property)
    {
        return await Task.FromResult(HtmlString.Empty);
    }
}