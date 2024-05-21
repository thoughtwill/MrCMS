using Microsoft.AspNetCore.Mvc.Rendering;
using MrCMS.Entities.Documents.Web.FormProperties;
using MrCMS.Settings;

namespace MrCMS.Shortcodes.Forms
{
    public class EmailRenderer : IFormElementRenderer<Email>
    {
        public TagBuilder AppendElement(Email formProperty, string existingValue, FormRenderingType formRenderingType)
        {
            var tagBuilder = new TagBuilder("input");
            tagBuilder.Attributes["type"] = "email";
            tagBuilder.Attributes["name"] = formProperty.Name;
            tagBuilder.Attributes["id"] = formProperty.GetHtmlId();
            tagBuilder.Attributes["placeholder"] = formProperty.PlaceHolder;


            if (formProperty.Required)
            {
                tagBuilder.Attributes["data-val"] = "true";
                tagBuilder.Attributes["data-val-required"] =
                    $"The field {(string.IsNullOrWhiteSpace(formProperty.LabelText)
                        ? formProperty.Name
                        : formProperty.LabelText)} is required";
                tagBuilder.Attributes["required"] = "required";
            }
            if (!string.IsNullOrWhiteSpace(formProperty.CssClass))
                tagBuilder.AddCssClass(formProperty.CssClass);
            
            tagBuilder.AddCssClass("form-control");

            tagBuilder.Attributes["value"] = existingValue;
            return tagBuilder;
        }

        public TagBuilder AppendElement(FormProperty formProperty, string existingValue, FormRenderingType formRenderingType)
        {
            return AppendElement(formProperty as Email, existingValue, formRenderingType);
        }

        public bool IsSelfClosing => true;
        public bool SupportsFloatingLabel => true;
    }
}