using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using MrCMS.Entities.Documents.Web;
using MrCMS.Mapping;
using MrCMS.Web.Admin.Infrastructure.Models.Tabs;

namespace MrCMS.Web.Admin.Models.Forms
{
    public class FormMessageTab : AdminTab<Form>
    {
        public override int Order
        {
            get { return 300; }
        }

        public override Type ParentType
        {
            get { return null; }
        }

        public override Type ModelType => typeof(FormMessageTabViewModel);

        public override string TabHtmlId
        {
            get { return "form-message-tab"; }
        }

        public override Task RenderTabPane(IHtmlHelper html, ISessionAwareMapper mapper, Form form)
        {
            return html.RenderPartialAsync("FormMessage", mapper.Map<FormMessageTabViewModel>(form));
        }

        public override Task<string> Name(IServiceProvider serviceProvider, Form entity)
        {
            return Task.FromResult("Settings");
        }

        public override Task<bool> ShouldShow(IServiceProvider serviceProvider, Form entity)
        {
            return Task.FromResult(true);
        }
    }
}