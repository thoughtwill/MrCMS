using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using MrCMS.Entities.Documents.Web;

namespace MrCMS.Shortcodes.Forms
{
    public class SubmittedMessageRenderer : ISubmittedMessageRenderer
    {
        public TagBuilder AppendSubmittedMessage(Form form, FormSubmittedStatus submittedStatus)
        {
            var message = new TagBuilder("div");
            message.AddCssClass("alert alert-dismissible fade show");
            message.AddCssClass(submittedStatus.Success ? "alert-success" : "alert-danger");
            message.InnerHtml.AppendHtml(
                (submittedStatus.Errors.Any()
                    ? RenderErrors(submittedStatus.Errors)
                    : (!string.IsNullOrWhiteSpace(form.FormSubmittedMessage)
                        ? form.FormSubmittedMessage
                        : "Form submitted")) + "<button type=\"button\" class=\"btn-close\" data-bs-dismiss=\"alert\" aria-label=\"Close\"></button>" );

            return message;
        }

        private string RenderErrors(List<string> errors)
        {
            var tagBuilder = new TagBuilder("ul");
            errors.ForEach(error => tagBuilder.InnerHtml.AppendHtml($"<li>{error}</li>"));
            return tagBuilder.GetString();
        }
    }
}