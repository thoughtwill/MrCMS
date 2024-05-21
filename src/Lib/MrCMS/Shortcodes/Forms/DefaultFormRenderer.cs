using System.Linq;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using MrCMS.Entities.Documents.Web;
using MrCMS.Services;
using MrCMS.Settings;
using MrCMS.Website.Filters;

namespace MrCMS.Shortcodes.Forms
{
    public class DefaultFormRenderer : IDefaultFormRenderer
    {
        private readonly IElementRendererManager _elementRendererManager;
        private readonly ILabelRenderer _labelRenderer;
        private readonly SiteSettings _siteSettings;
        private readonly IGetCurrentPage _getCurrentPage;
        private readonly ISubmittedMessageRenderer _submittedMessageRenderer;
        private readonly IValidationMessaageRenderer _validationMessageRenderer;

        public DefaultFormRenderer(IElementRendererManager elementRendererManager, ILabelRenderer labelRenderer,
            IValidationMessaageRenderer validationMessageRenderer, ISubmittedMessageRenderer submittedMessageRenderer,
            SiteSettings siteSettings, IGetCurrentPage getCurrentPage)
        {
            _elementRendererManager = elementRendererManager;
            _labelRenderer = labelRenderer;
            _validationMessageRenderer = validationMessageRenderer;
            _submittedMessageRenderer = submittedMessageRenderer;
            _siteSettings = siteSettings;
            _getCurrentPage = getCurrentPage;
        }

        public IHtmlContent GetDefault(IHtmlHelper helper, Form formEntity, FormSubmittedStatus submittedStatus)
        {
            if (formEntity == null)
            {
                return HtmlString.Empty;
            }

            var formProperties = formEntity.FormProperties.OrderBy(x => x.DisplayOrder).ToList();
            if (!formProperties.Any())
            {
                return HtmlString.Empty;
            }

            var form = GetForm(formEntity);
            var renderingType = _siteSettings.FormRendererType;
            var labelRenderingType = _siteSettings.FormLabelRenderingType;
            if (submittedStatus.Submitted)
            {
                form.InnerHtml.AppendHtml(new TagBuilder("br"));
                form.InnerHtml.AppendHtml(
                    _submittedMessageRenderer.AppendSubmittedMessage(formEntity, submittedStatus));
            }

            foreach (var property in formProperties)
            {
                IHtmlContentBuilder elementHtml = new HtmlContentBuilder();
                var renderer = _elementRendererManager.GetPropertyRenderer(property);
                var existingValue = submittedStatus.Data[property.Name];

                var element = renderer.AppendElement(property, existingValue, renderingType);
                element.TagRenderMode = renderer.IsSelfClosing ? TagRenderMode.SelfClosing : TagRenderMode.Normal;

                if (renderer.SupportsFloatingLabel && labelRenderingType == FormLabelRenderingType.Floating)
                {
                    elementHtml.AppendHtml(element);
                    elementHtml.AppendHtml(_labelRenderer.AppendLabel(property));
                }
                else
                {
                    elementHtml.AppendHtml(_labelRenderer.AppendLabel(property));
                    elementHtml.AppendHtml(element);
                }

                elementHtml.AppendHtml(_validationMessageRenderer.AppendRequiredMessage(property));

                var elementContainer =
                    _elementRendererManager.GetPropertyContainer(renderingType,
                        renderer.SupportsFloatingLabel ? labelRenderingType : FormLabelRenderingType.Normal, property);
                if (elementContainer != null)
                {
                    elementContainer.InnerHtml.AppendHtml(elementHtml);
                    form.InnerHtml.AppendHtml(elementContainer);
                }
                else
                {
                    form.InnerHtml.AppendHtml(elementHtml);
                }
            }

            if (formEntity.ShowGDPRConsentBox)
            {
                form.InnerHtml.AppendHtml(GetGDPRCheckbox(renderingType, _siteSettings.GDPRFairProcessingText));
            }

            form.InnerHtml.AppendHtml(helper.AntiForgeryToken());

            form.InnerHtml.AppendHtml(helper.RenderRecaptcha());

            var div = new TagBuilder("div");
            div.InnerHtml.AppendHtml(GetSubmitButton(formEntity));
            form.InnerHtml.AppendHtml(div);

            if (_siteSettings.HasHoneyPot)
            {
                form.InnerHtml.AppendHtml(_siteSettings.GetHoneypot());
            }

            form.InnerHtml.AppendHtml(GetReturnUrlInput());

            return form;
        }

        public static IHtmlContent GetGDPRCheckbox(FormRenderingType renderingType, string labelText)
        {
            var cbLabelBuilder = new TagBuilder("label");
            var sanitizedId = TagBuilder.CreateSanitizedId(FormPostingHandler.GDPRConsent, "-");
            cbLabelBuilder.Attributes["for"] = sanitizedId;


            var checkboxBuilder = new TagBuilder("input")
            {
                Attributes =
                {
                    ["type"] = "checkbox",
                    ["value"] = "true"
                }
            };

            var requiredMessage = "GDPR Consent is required";
            checkboxBuilder.Attributes["data-val"] = "true";
            checkboxBuilder.Attributes["data-val-mandatory"] = null;
            checkboxBuilder.Attributes["data-val-required"] = requiredMessage;
            checkboxBuilder.Attributes["data-val-required"] = requiredMessage;
            checkboxBuilder.Attributes["required"] = "required";
            checkboxBuilder.Attributes["name"] = FormPostingHandler.GDPRConsent;
            checkboxBuilder.Attributes["id"] = sanitizedId;
            checkboxBuilder.AddCssClass("form-check-input");

            var checkboxContainer = new TagBuilder("div");

            cbLabelBuilder.InnerHtml.AppendHtml(labelText);
            cbLabelBuilder.AddCssClass("form-check-label");
            checkboxContainer.AddCssClass("form-check");
            checkboxContainer.InnerHtml.AppendHtml(checkboxBuilder);
            checkboxContainer.InnerHtml.AppendHtml(cbLabelBuilder);
            var wrapper = new TagBuilder("div");
            wrapper.AddCssClass("form-group");
            wrapper.InnerHtml.AppendHtml(checkboxContainer);
            wrapper.InnerHtml.AppendHtml(
                ValidationMessaageRenderer.GetValidationMessage(FormPostingHandler.GDPRConsent));
            return wrapper;
        }

        public TagBuilder GetSubmitButton(Form form)
        {
            var tagBuilder = new TagBuilder("input")
            {
                TagRenderMode = TagRenderMode.SelfClosing,
                Attributes =
                {
                    ["type"] = "submit",
                    ["value"] = !string.IsNullOrWhiteSpace(form.SubmitButtonText)
                        ? form.SubmitButtonText
                        : "Submit"
                }
            };
            tagBuilder.AddCssClass(!string.IsNullOrWhiteSpace(form.SubmitButtonCssClass)
                ? form.SubmitButtonCssClass
                : "btn btn-primary");
            return tagBuilder;
        }

        public TagBuilder GetForm(Form form)
        {
            var tagBuilder = new TagBuilder("form")
            {
                Attributes =
                {
                    ["method"] = "POST",
                    ["enctype"] = "multipart/form-data",
                    ["action"] = $"/save-form/{form.Id}",
                    ["novalidate"] = "true"
                }
            };

            return tagBuilder;
        }

        private TagBuilder GetReturnUrlInput()
        {
            var currentPage = _getCurrentPage.GetPage();
            var returnUrlInput = new TagBuilder("input");
            returnUrlInput.Attributes["type"] = "hidden";
            returnUrlInput.Attributes["name"] = "returnUrl";
            returnUrlInput.Attributes["novalidate"] = "true";
            returnUrlInput.TagRenderMode = TagRenderMode.SelfClosing;
            returnUrlInput.Attributes["value"] = $"/{currentPage?.UrlSegment}";
            return returnUrlInput;
        }
    }
}