using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using MrCMS.Entities.Documents.Web;
using MrCMS.Entities.Documents.Web.FormProperties;
using MrCMS.Services;
using MrCMS.Settings;
using MrCMS.Website.Filters;

namespace MrCMS.Shortcodes.Forms
{
    public class CustomFormRenderer : ICustomFormRenderer
    {
        private readonly IElementRendererManager _elementRendererManager;
        private readonly ILabelRenderer _labelRenderer;
        private readonly IValidationMessaageRenderer _validationMessaageRenderer;
        private readonly ISubmittedMessageRenderer _submittedMessageRenderer;
        private readonly SiteSettings _siteSettings;
        private readonly IGetCurrentPage _getCurrentPage;

        public CustomFormRenderer(IElementRendererManager elementRendererManager, ILabelRenderer labelRenderer,
                                  IValidationMessaageRenderer validationMessaageRenderer,
                                  ISubmittedMessageRenderer submittedMessageRenderer, SiteSettings siteSettings, IGetCurrentPage getCurrentPage)
        {
            _elementRendererManager = elementRendererManager;
            _labelRenderer = labelRenderer;
            _validationMessaageRenderer = validationMessaageRenderer;
            _submittedMessageRenderer = submittedMessageRenderer;
            _siteSettings = siteSettings;
            _getCurrentPage = getCurrentPage;
        }

        public IHtmlContent GetForm(IHtmlHelper helper, Form form1, FormSubmittedStatus submittedStatus)
        {
            if (form1 == null)
                return HtmlString.Empty;

            var formProperties = form1.FormProperties;
            if (!formProperties.Any())
                return HtmlString.Empty;

            var form = GetForm(form1);

            var formDesign = form1.FormDesign;
            formDesign = Regex.Replace(formDesign, "{label:([^}]+)}", AddLabel(formProperties));
            formDesign = Regex.Replace(formDesign, "{input:([^}]+)}", AddElement(formProperties, submittedStatus));
            formDesign = Regex.Replace(formDesign, "{validation:([^}]+)}", AddValidation(formProperties));
            formDesign = Regex.Replace(formDesign, "{submitted-message}", AddSubmittedMessage(form1, submittedStatus));
            formDesign = Regex.Replace(formDesign, "{recaptcha}", helper.RenderRecaptcha().GetString());
            formDesign = Regex.Replace(formDesign, "{gdpr}", DefaultFormRenderer.GetGDPRCheckbox(_siteSettings.FormRendererType, _siteSettings.GDPRFairProcessingText).GetString());
            form.InnerHtml.AppendHtml(formDesign);
            form.InnerHtml.AppendHtml(GetReturnUrlInput());

            if (_siteSettings.HasHoneyPot)
                form.InnerHtml.AppendHtml(_siteSettings.GetHoneypot());
            
            
  
            return form;
        }

        private MatchEvaluator AddValidation(IList<FormProperty> formProperties)
        {
            return match =>
            {

                var formProperty =
                    formProperties.FirstOrDefault(
                        property => property.Name.Equals(match.Groups[1].Value, StringComparison.OrdinalIgnoreCase));
                return formProperty == null
                    ? string.Empty
                    : _validationMessaageRenderer.AppendRequiredMessage(formProperty).GetString();
            };
        }
        
        private TagBuilder GetReturnUrlInput()
        {
            var currentPage = _getCurrentPage.GetPage();
            var returnUrlInput = new TagBuilder("input");
            returnUrlInput.Attributes["type"] = "hidden";
            returnUrlInput.Attributes["name"] = "returnUrl";
            returnUrlInput.TagRenderMode = TagRenderMode.SelfClosing;
            returnUrlInput.Attributes["value"] =$"/{currentPage?.UrlSegment}";
            return returnUrlInput;
        }

        private string AddSubmittedMessage(Form form, FormSubmittedStatus submittedStatus)
        {
            if (!submittedStatus.Submitted) return string.Empty;

            return _submittedMessageRenderer.AppendSubmittedMessage(form, submittedStatus).GetString();
        }

        private MatchEvaluator AddLabel(IList<FormProperty> formProperties)
        {
            return match =>
                       {
                           var formProperty =
                               formProperties.FirstOrDefault(
                                   property => property.Name.Equals(match.Groups[1].Value, StringComparison.OrdinalIgnoreCase));
                           return formProperty == null
                               ? string.Empty
                               : _labelRenderer.AppendLabel(formProperty).GetString();
                       };
        }
        private MatchEvaluator AddElement(IList<FormProperty> formProperties, FormSubmittedStatus submittedStatus)
        {
            return match =>
                       {

                           var formProperty =
                               formProperties.FirstOrDefault(
                                   property => property.Name.Equals(match.Groups[1].Value, StringComparison.OrdinalIgnoreCase));
                           if (formProperty == null)
                               return string.Empty;
                           var existingValue = submittedStatus.Data[formProperty.Name];

                           var renderer = _elementRendererManager.GetPropertyRenderer(formProperty);

                           var element = renderer.AppendElement(formProperty, existingValue, _siteSettings.FormRendererType);
                           element.TagRenderMode = renderer.IsSelfClosing
                               ? TagRenderMode.SelfClosing
                               : TagRenderMode.Normal;
                           return element.GetString();
                       };
        }

        public TagBuilder GetForm(Form form)
        {
            var tagBuilder = new TagBuilder("form");
            tagBuilder.Attributes["method"] = "POST";
            tagBuilder.Attributes["enctype"] = "multipart/form-data";
            tagBuilder.Attributes["novalidate"] = "true";
            tagBuilder.Attributes["action"] = $"/save-form/{form.Id}";

            return tagBuilder;
        } 
    }
}