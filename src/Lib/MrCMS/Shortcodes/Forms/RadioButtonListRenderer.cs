using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using MrCMS.Entities.Documents.Web.FormProperties;
using MrCMS.Settings;

namespace MrCMS.Shortcodes.Forms
{
    public class RadioButtonListRenderer : IFormElementRenderer<RadioButtonList>
    {
        public TagBuilder AppendElement(RadioButtonList formProperty, string existingValue,
            FormRenderingType formRenderingType)
        {
            var values = existingValue == null
                ? new List<string>()
                : existingValue.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .ToList();

            var tagBuilder = new TagBuilder("div");
            tagBuilder.AddCssClass("mt-2");
            
            foreach (var radioOption in formProperty.Options)
            {
                var cbLabelBuilder = new TagBuilder("label")
                {
                    Attributes =
                    {
                        ["for"] = TagBuilder.CreateSanitizedId(formProperty.Name + "-" + radioOption.Value, "-")
                    }
                };

                var checkboxBuilder = GetRadioButton(formProperty, existingValue, radioOption, values);

                cbLabelBuilder.InnerHtml.AppendHtml(radioOption.Value);
                var checkboxContainer = new TagBuilder("div");
                cbLabelBuilder.AddCssClass("form-check-label");
                checkboxContainer.AddCssClass("form-check");
                checkboxContainer.InnerHtml.AppendHtml(checkboxBuilder);
                checkboxContainer.InnerHtml.AppendHtml(cbLabelBuilder);
                tagBuilder.InnerHtml.AppendHtml(checkboxContainer);
            }
            

            return tagBuilder;
        }
        
        private static TagBuilder GetRadioButton(RadioButtonList formProperty, string existingValue, FormListOption radioOption,
            List<string> values)
        {
            var radioButtonBuilder = new TagBuilder("input")
            {
                Attributes =
                {
                    ["type"] = "radio",
                    ["value"] = radioOption.Value
                }
            };
            radioButtonBuilder.AddCssClass("form-check-input");
            radioButtonBuilder.AddCssClass(formProperty.CssClass);

            if (existingValue != null)
            {
                if (values.Contains(radioOption.Value))
                    radioButtonBuilder.Attributes["checked"] = "checked";
            }
            else if (radioOption.Selected)
                radioButtonBuilder.Attributes["checked"] = "checked";

            if (formProperty.Required)
            {
                var requiredMessage =
                    $"The field {(string.IsNullOrWhiteSpace(formProperty.LabelText) ? formProperty.Name : formProperty.LabelText)} is required";
                radioButtonBuilder.Attributes["data-val"] = "true";
                radioButtonBuilder.Attributes["data-val-mandatory"] = requiredMessage;
                radioButtonBuilder.Attributes["data-val-required"] = requiredMessage;
                radioButtonBuilder.Attributes["required"] = "required";
            }

            radioButtonBuilder.Attributes["name"] = formProperty.Name;
            radioButtonBuilder.Attributes["id"] =
                TagBuilder.CreateSanitizedId(formProperty.Name + "-" + radioOption.Value, "-");
            return radioButtonBuilder;
        }

        public TagBuilder AppendElement(FormProperty formProperty, string existingValue,
            FormRenderingType formRenderingType)
        {
            return AppendElement(formProperty as RadioButtonList, existingValue, formRenderingType);
        }

        public bool IsSelfClosing => false;
        public bool SupportsFloatingLabel => false;
    }
}