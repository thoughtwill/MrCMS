using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using MrCMS.Entities.Documents.Web;
using MrCMS.Services;
using MrCMS.Tests.Website.Controllers.Builders;
using Xunit;

namespace MrCMS.Tests.Website.Controllers
{
    public class WebpageControllerTests
    {
        [Fact]
        public async Task Show_ReturnsThePassedWebpageAsAViewResult()
        {
            var service = A.Fake<IWebpageUIService>();
            var webpage = A.Dummy<Webpage>();
            A.CallTo(() => service.GetPage<Webpage>(123)).Returns(webpage);
            var controller = new WebpageControllerBuilder().WithUiService(service).Build();

            var result = await controller.Show(123);

            result.Should().BeOfType<ViewResult>();
            //result.Model.Should().Be(webpage);
        }
    }
}