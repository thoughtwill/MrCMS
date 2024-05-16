using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using MrCMS.Entities.Documents.Web;
using MrCMS.Mapping;
using MrCMS.Web.Admin.Infrastructure.Models.Tabs;

namespace MrCMS.Web.Admin.Models.Forms
{
    public class FormGDPRSettingsTab : AdminTab<Form>
    {
        public override int Order
        {
            get { return 400; }
        }

        public override Type ParentType
        {
            get { return null; }
        }

        public override Type ModelType => typeof(FormGDPRTabViewModel);

        public override string TabHtmlId
        {
            get { return "form-gdpr-tab"; }
        }

        public override Task RenderTabPane(IHtmlHelper html, ISessionAwareMapper mapper, Form form)
        {
            return html.RenderPartialAsync("GDPR", mapper.Map<FormGDPRTabViewModel>(form));
        }

        public override Task<string> Name(IServiceProvider serviceProvider, Form entity)
        {
            return Task.FromResult("GDPR");
        }

        public override Task<bool> ShouldShow(IServiceProvider serviceProvider, Form entity)
        {
            return Task.FromResult(true);
        }
    }
}