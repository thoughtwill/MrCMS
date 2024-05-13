using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using MrCMS.Entities.Documents.Web;
using MrCMS.Shortcodes.Forms;
using NHibernate;
using System.Collections.Generic;
using System.Collections.Specialized;
using MrCMS.Helpers;

namespace MrCMS.Shortcodes
{
    public class FormShortcodeRenderer : IShortcodeRenderer
    {
        private readonly IFormRenderer _formRenderer;
        private readonly ISession _session;

        public FormShortcodeRenderer(ISession session, IFormRenderer formRenderer)
        {
            _session = session;
            _formRenderer = formRenderer;
        }

        public string TagName => "form";

        public IHtmlContent Render(IHtmlHelper helper, Dictionary<string, string> attributes)
        {
            if (!attributes.ContainsKey("id"))
            {
                return HtmlString.Empty;
            }

            if (!int.TryParse(attributes["id"], out var id))
            {
                return HtmlString.Empty;
            }

            var form = _session.Get<Form>(id);
            if (form == null)
            {
                return HtmlString.Empty;
            }

            var status = GetStatus(helper.ViewContext);
            return _formRenderer.RenderForm(helper, form, status);
        }

        private static FormSubmittedStatus GetStatus(ViewContext viewContext)
        {
            var submitted = true.Equals(viewContext.TempData["form-submitted"]);
            var errors = viewContext.TempData.Get<List<string>>("form-submitted-message");
            var data = viewContext.TempData.Get<NameValueCollection>("form-data");
            var status = new FormSubmittedStatus(submitted, errors, data);
            return status;
        }
    }
}