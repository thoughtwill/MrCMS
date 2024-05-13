using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using MrCMS.Entities.Documents.Web;
using MrCMS.Services;
using MrCMS.TestSupport;
using MrCMS.Website.Controllers;
using Xunit;

namespace MrCMS.Tests.Website.Controllers
{
    public class FormControllerTests
    {
        private readonly FormController _formController;
        private readonly IFormPostingHandler _formPostingHandler;

        public FormControllerTests()
        {
            _formPostingHandler = A.Fake<IFormPostingHandler>();
            _formController = new FormController(_formPostingHandler)
            {
                TempData = new MockTempDataDictionary(),
                ControllerContext = new ControllerContext()

                //RequestMock =
                //    A.Fake<HttpRequestBase>(),
                //ReferrerOverride = "http://www.example.com/test-redirect"
            };
        }

        [Fact]
        public async Task FormController_Save_CallsFormServiceSaveFormDataWithPassedObjects()
        {
            // Arrange
            var form = new Form();
            A.CallTo(() => _formPostingHandler.GetForm(123)).Returns(form);

            var httpContext = new DefaultHttpContext(); // Create a new HttpContext.
            var request = httpContext.Request;
            request.Form = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                { "returnUrl", "/" } // Assume returnUrl is accessed in the Save method.
            });

            // Set up the controller context to include the mock HttpContext
            _formController.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            // Act
            ActionResult result = await _formController.Save(123);

            // Assert
            A.CallTo(() => _formPostingHandler.SaveFormData(form, _formController.Request)).MustHaveHappened();
        }

        [Fact]
        public async Task FormController_Save_SetsTempDataFormSubmittedToTrue()
        {
            // Arrange
            var form = new Form();
            A.CallTo(() => _formPostingHandler.GetForm(123)).Returns(form);

            var httpContext = new DefaultHttpContext(); // Create a new HttpContext.
            var request = httpContext.Request;
            request.Form = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                { "returnUrl", "/" }
            });

            _formController.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            // Act
            ActionResult result = await _formController.Save(123);

            // Assert
            _formController.TempData["form-submitted"].Should().Be(true);
        }

        [Fact]
        public async Task FormController_Save_RedirectsToProvidedReturnUrl()
        {
            // Arrange
            var form = new Form { FormRedirectUrl = string.Empty }; // Assuming FormRedirectUrl can be empty indicating no specific redirection.
            var returnUrl = "/customReturnUrl";
            var defaultFormCollection = new FormCollection(new Dictionary<string, StringValues>
            {
                { "returnUrl", returnUrl }
            });

            // Mocking the GetForm to return a non-deleted form
            A.CallTo(() => _formPostingHandler.GetForm(123)).Returns(form);

            // Setup to ensure Request.Form can be accessed and returns our simulated form collection
            // Assuming _formController.ControllerContext is already set up for mocking if needed
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Form = defaultFormCollection;
            _formController.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            ActionResult result = await _formController.Save(123);

            // Assert
            result.Should().BeOfType<RedirectResult>();
            var redirectResult = result as RedirectResult;
            redirectResult.Url.Should().Be(returnUrl);
        }
    }
}