using Microsoft.AspNetCore.Mvc.Rendering;
using MrCMS.Entities.Documents.Web.FormProperties;
using MrCMS.Settings;

namespace MrCMS.Shortcodes.Forms
{
    public interface IElementRendererManager
    {
        IFormElementRenderer GetPropertyRenderer<T>(T property) where T : FormProperty;
        TagBuilder GetPropertyContainer(FormRenderingType formRenderingType, FormLabelRenderingType formLabelRenderingType, FormProperty property);
    }
}