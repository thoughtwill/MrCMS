using FluentAssertions;
using MrCMS.Entities.Documents.Web;
using MrCMS.Shortcodes.Forms;
using Xunit;

namespace MrCMS.Tests.Shortcodes.Forms
{
    public class SubmittedMessageRendererTests
    {
        private SubmittedMessageRenderer _submittedMessageRenderer;

        public SubmittedMessageRendererTests()
        {
            _submittedMessageRenderer = new SubmittedMessageRenderer();
        }

        [Fact]
        public void SubmittedMessageRenderer_GetSubmittedMessage_ReturnsADiv()
        {
            var submittedMessage = _submittedMessageRenderer.AppendSubmittedMessage(new Form(), new FormSubmittedStatus(true, null, null));

            submittedMessage.TagName.Should().Be("div");
        }

        [Fact]
        public void SubmittedMessageRenderer_GetSubmittedMessage_ShouldHaveAlertAndAlertSuccessClasses()
        {
            var submittedMessage = _submittedMessageRenderer.AppendSubmittedMessage(new Form(), new FormSubmittedStatus(true, null, null));

            submittedMessage.Attributes["class"].Should().Contain("alert-success");
        }
    }
}