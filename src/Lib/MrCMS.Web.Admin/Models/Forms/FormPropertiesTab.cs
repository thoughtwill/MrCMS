using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using MrCMS.Entities.Documents.Web;
using MrCMS.Mapping;
using MrCMS.Web.Admin.Infrastructure.Models.Tabs;

namespace MrCMS.Web.Admin.Models.Forms
{
    public class FormPropertiesTab : AdminTab<Form>
    {
        public override int Order => 0;

        public override Task<string> Name(IServiceProvider serviceProvider, Form entity)
        {
            return Task.FromResult("Fields");
        }

        public override Task<bool> ShouldShow(IServiceProvider serviceProvider, Form entity)
        {
            return Task.FromResult(true);
        }

        public override Type ParentType => null;

        public override Type ModelType => null;

        public override string TabHtmlId
        {
            get { return "form-properties-tab"; }
        }

        public override Task RenderTabPane(IHtmlHelper html, ISessionAwareMapper mapper, Form form)
        {
            return html.RenderPartialAsync("FormProperties", form);
        }
    }
}