using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using MrCMS.Entities.Documents.Web;
using MrCMS.Mapping;
using MrCMS.Web.Admin.Infrastructure.Models.Tabs;

namespace MrCMS.Web.Admin.Models.Forms
{
    public class FormDesignTab : AdminTab<Form>
    {
        public override int Order
        {
            get { return 100; }
        }

        public override Task<string> Name(IServiceProvider serviceProvider, Form entity)
        {
            return Task.FromResult("Design");
        }

        public override Task<bool> ShouldShow(IServiceProvider serviceProvider, Form entity)
        {
            return Task.FromResult(true);
        }

        public override Type ParentType
        {
            get { return null; }
        }

        public override Type ModelType => typeof(FormDesignTabViewModel);

        public override string TabHtmlId
        {
            get { return "form-design-tab"; }
        }

        public override Task RenderTabPane(IHtmlHelper html, ISessionAwareMapper mapper, Form webpage)
        {
            return html.RenderPartialAsync("FormDesign", mapper.Map<FormDesignTabViewModel>(webpage));
        }
    }
}