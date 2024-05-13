using Microsoft.AspNetCore.Mvc.Rendering;
using MrCMS.Entities.Documents.Web.FormProperties;
using MrCMS.Settings;

namespace MrCMS.Shortcodes.Forms
{
    public interface IFormElementRenderer<in T> : IFormElementRenderer where T : FormProperty
    {
        TagBuilder AppendElement(T formProperty, string existingValue, FormRenderingType formRenderingType);
    }

    public interface IFormElementRenderer
    {
        bool IsSelfClosing { get; }
        bool SupportsFloatingLabel { get; }
        TagBuilder AppendElement(FormProperty formProperty, string existingValue, FormRenderingType formRenderingType);
    }
}