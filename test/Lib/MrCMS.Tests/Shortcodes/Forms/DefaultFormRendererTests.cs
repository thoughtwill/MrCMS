using FakeItEasy;
using FakeItEasy.Core;
using FluentAssertions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using MrCMS.Entities.Documents.Web;
using MrCMS.Entities.Documents.Web.FormProperties;
using MrCMS.Settings;
using MrCMS.Shortcodes.Forms;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using MrCMS.Services;
using Xunit;

namespace MrCMS.Tests.Shortcodes.Forms
{
    public class DefaultFormRendererTests
    {
        private readonly DefaultFormRenderer _defaultFormRenderer;
        private readonly IElementRendererManager _elementRendererManager;
        private readonly IGetCurrentPage _getCurrentPage;
        private readonly ILabelRenderer _labelRenderer;
        private readonly IValidationMessaageRenderer _validationMessageRenderer;
        private readonly string _existingValue = null;
        private readonly NameValueCollection _formCollection;
        private readonly ISubmittedMessageRenderer _submittedMessageRenderer;
        private readonly SiteSettings _siteSettings;
        private readonly IHtmlHelper _htmlHelper;

        public DefaultFormRendererTests()
        {
            _formCollection = new NameValueCollection();
            _elementRendererManager = A.Fake<IElementRendererManager>();
            _labelRenderer = A.Fake<ILabelRenderer>();
            _validationMessageRenderer = A.Fake<IValidationMessaageRenderer>();
            _submittedMessageRenderer = A.Fake<ISubmittedMessageRenderer>();
            _getCurrentPage = A.Fake<IGetCurrentPage>();
            _htmlHelper = A.Fake<IHtmlHelper>();
            _siteSettings = new SiteSettings();
            _defaultFormRenderer = new DefaultFormRenderer(_elementRendererManager, _labelRenderer,
                                                           _validationMessageRenderer, _submittedMessageRenderer, _siteSettings, _getCurrentPage);
        }

        [Fact]
        public void DefaultFormRenderer_GetDefault_ShouldReturnAnEmptyStringIfThereAreNoProperties()
        {
            var form = new Form();

            var @default = _defaultFormRenderer.GetDefault(_htmlHelper, form, new FormSubmittedStatus(false, null, null));

            @default.Should().Be(HtmlString.Empty);
        }

        [Fact]
        public void DefaultFormRenderer_GetDefault_ShouldReturnAnEmptyStringIfFormIsNull()
        {
            var @default = _defaultFormRenderer.GetDefault(_htmlHelper, null, new FormSubmittedStatus(false, null, null));

            @default.Should().Be(HtmlString.Empty);
        }

        [Fact(Skip = "Refactor with injectable recaptcha")]
        public void DefaultFormRenderer_GetDefault_ShouldCallGetElementRendererOnEachProperty()
        {
            var textBox = new TextBox { Name = "test-1" };
            var form = new Form
            {
                FormProperties = new List<FormProperty> { textBox }
            };
            var formElementRenderer = A.Fake<IFormElementRenderer>();
            A.CallTo(() => _elementRendererManager.GetPropertyRenderer<FormProperty>(textBox))
             .Returns(formElementRenderer);
            A.CallTo(() => formElementRenderer.AppendElement(textBox, _existingValue, _siteSettings.FormRendererType)).Returns(new TagBuilder("input"));
            //A.CallTo(() => _htmlHelper.Action("Render", "Form", A<object>._)).Throws<Exception>();

            _defaultFormRenderer.GetDefault(_htmlHelper, form, new FormSubmittedStatus(false, null, _formCollection));

            A.CallTo(() => _elementRendererManager.GetPropertyRenderer<FormProperty>(textBox)).MustHaveHappened();
        }


        [Fact(Skip = "Refactor with injectable recaptcha")]
        public void DefaultFormRenderer_GetDefault_ShouldCallAppendLabelOnLabelRendererForEachProperty()
        {
            var textBox = new TextBox { Name = "test-1" };
            var form = new Form
            {
                FormProperties = new List<FormProperty> { textBox }
            };
            var formElementRenderer = A.Fake<IFormElementRenderer>();
            A.CallTo(() => _elementRendererManager.GetPropertyRenderer<FormProperty>(textBox))
             .Returns(formElementRenderer);
            A.CallTo(() => formElementRenderer.AppendElement(textBox, _existingValue, _siteSettings.FormRendererType)).Returns(new TagBuilder("input"));

            _defaultFormRenderer.GetDefault(_htmlHelper, form, new FormSubmittedStatus(false, null, _formCollection));

            A.CallTo(() => _labelRenderer.AppendLabel(textBox)).MustHaveHappened();
        }

        [Fact(Skip = "Refactor with injectable recaptcha")]
        public void DefaultFormRenderer_GetDefault_ShouldCallAppendControlOnElementRenderer()
        {
            var textBox = new TextBox { Name = "test-1" };
            var form = new Form
            {
                FormProperties = new List<FormProperty> { textBox }
            };
            var formElementRenderer = A.Fake<IFormElementRenderer>();
            A.CallTo(() => _elementRendererManager.GetPropertyRenderer<FormProperty>(textBox))
             .Returns(formElementRenderer);
            A.CallTo(() => formElementRenderer.AppendElement(textBox, _existingValue, _siteSettings.FormRendererType)).Returns(new TagBuilder("input"));

            _defaultFormRenderer.GetDefault(_htmlHelper, form, new FormSubmittedStatus(false, null, _formCollection));

            A.CallTo(() => formElementRenderer.AppendElement(textBox, _existingValue, _siteSettings.FormRendererType)).MustHaveHappened();
        }

        [Fact(Skip = "Refactor with injectable recaptcha")]
        public void DefaultFormRenderer_GetDefault_ShouldCallRenderLabelThenRenderElementForEachProperty()
        {
            var textBox1 = new TextBox { Name = "test-1" };
            var textBox2 = new TextBox { Name = "test-2" };

            var form = new Form
            {
                FormProperties = new List<FormProperty> { textBox1, textBox2 }
            };
            var formElementRenderer = A.Fake<IFormElementRenderer>();
            A.CallTo(() => formElementRenderer.AppendElement(textBox1, _existingValue, _siteSettings.FormRendererType)).Returns(new TagBuilder("input"));
            A.CallTo(() => formElementRenderer.AppendElement(textBox2, _existingValue, _siteSettings.FormRendererType)).Returns(new TagBuilder("input"));
            A.CallTo(() => _elementRendererManager.GetPropertyRenderer<FormProperty>(textBox1))
             .Returns(formElementRenderer);
            A.CallTo(() => _elementRendererManager.GetPropertyRenderer<FormProperty>(textBox2))
             .Returns(formElementRenderer);

            _defaultFormRenderer.GetDefault(_htmlHelper, form, new FormSubmittedStatus(false, null, _formCollection));

            List<ICompletedFakeObjectCall> elementRendererCalls = Fake.GetCalls(formElementRenderer).ToList();
            List<ICompletedFakeObjectCall> labelRendererCalls = Fake.GetCalls(_labelRenderer).ToList();

            labelRendererCalls.Where(x => x.Method.Name == "AppendLabel").Should().HaveCount(2);
            elementRendererCalls.Where(x => x.Method.Name == "AppendElement").Should().HaveCount(2);
        }


        [Fact(Skip = "Refactor with injectable recaptcha")]
        public void DefaultFormRenderer_GetDefault_ReturnsFormRenderIfItRenders()
        {
            var textBox1 = new TextBox { Name = "test-1" };
            var textBox2 = new TextBox { Name = "test-2" };

            var form = new Form
            {
                FormProperties = new List<FormProperty> { textBox1, textBox2 }
            };
            //A.CallTo(() => _htmlHelper.Action("Render", "Form", A<object>._)).Returns(MvcHtmlString.Create("rendered"));

            var result = _defaultFormRenderer.GetDefault(_htmlHelper, form, new FormSubmittedStatus(false, null, _formCollection));

            result.ToString().Should().Be("rendered");
        }

        [Fact]
        public void DefaultFormRenderer_GetForm_ShouldHaveTagTypeOfForm()
        {
            var tagBuilder = _defaultFormRenderer.GetForm(new Form());

            tagBuilder.TagName.Should().Be("form");
        }

        [Fact]
        public void DefaultFormRenderer_GetForm_ShouldHaveMethodPost()
        {
            var tagBuilder = _defaultFormRenderer.GetForm(new Form());
            tagBuilder.Attributes["method"].Should().Be("POST");
        }

        [Fact]
        public void DefaultFormRenderer_GetForm_ShouldHaveActionSaveFormWithTheIdPassed()
        {
            var tagBuilder = _defaultFormRenderer.GetForm(new Form
            {
                Id = 123
            });
            tagBuilder.Attributes["action"].Should().Be("/save-form/123");
        }

        [Fact]
        public void DefaultFormRenderer_GetSubmitButton_ShouldReturnAnInput()
        {
            var submitButton = _defaultFormRenderer.GetSubmitButton(new Form
            {
            });

            submitButton.TagName.Should().Be("input");
        }

        [Fact]
        public void DefaultFormRenderer_GetSubmitButton_ShouldBeOfTypeSubmit()
        {
            var submitButton = _defaultFormRenderer.GetSubmitButton(new Form
            {
            });

            submitButton.Attributes["type"].Should().Be("submit");
        }

        [Fact]
        public void DefaultFormRenderer_GetSubmitButton_ValueShouldBeSubmitForm()
        {
            var submitButton = _defaultFormRenderer.GetSubmitButton(new Form
            {
            });

            submitButton.Attributes["value"].Should().Be("Submit");
        }

        [Fact]
        public void DefaultFormRenderer_GetSubmitButton_CssClassShouldBeCustomIfSet()
        {
            var submitButton = _defaultFormRenderer.GetSubmitButton(new Form
            {
                SubmitButtonCssClass = "my-css-button-class"
            });

            submitButton.Attributes["class"].Should().Be("my-css-button-class");
        }
    }
}