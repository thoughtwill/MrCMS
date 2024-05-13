using Microsoft.AspNetCore.Mvc.Rendering;
using MrCMS.Entities.Documents.Web.FormProperties;
using MrCMS.Settings;

namespace MrCMS.Shortcodes.Forms
{
    public class DropDownListRenderer : IFormElementRenderer<DropDownList>
    {
        private readonly IDropDownListOptionRenderer _dropDownListOptionRenderer;

        public DropDownListRenderer(IDropDownListOptionRenderer dropDownListOptionRenderer)
        {
            _dropDownListOptionRenderer = dropDownListOptionRenderer;
        }

        public TagBuilder AppendElement(DropDownList formProperty, string existingValue, FormRenderingType formRenderingType)
        {
            var tagBuilder = new TagBuilder("select");
            tagBuilder.Attributes["name"] = formProperty.Name;
            tagBuilder.Attributes["id"] = formProperty.GetHtmlId();

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
            if (formRenderingType == FormRenderingType.Bootstrap4)
                tagBuilder.AddCssClass("form-control");
            else if (formRenderingType is FormRenderingType.Bootstrap5) 
                tagBuilder.AddCssClass("form-select");

            foreach (var option in formProperty.Options)
            {
                tagBuilder.InnerHtml.AppendHtml(_dropDownListOptionRenderer.GetOption(option, existingValue));
            }
            return tagBuilder;
        }

        public TagBuilder AppendElement(FormProperty formProperty, string existingValue, FormRenderingType formRenderingType)
        {
            return AppendElement(formProperty as DropDownList, existingValue, formRenderingType);
        }

        public bool IsSelfClosing => false;
        public bool SupportsFloatingLabel => true;
    }
}