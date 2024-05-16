using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using MrCMS.Entities.Documents.Web;
using MrCMS.Mapping;
using MrCMS.Web.Admin.Infrastructure.Models.Tabs;

namespace MrCMS.Web.Admin.Models.WebpageEdit
{
    public class LayoutTab : AdminTab<Webpage>
    {
        public override int Order
        {
            get { return 200; }
        }

        public override Type ParentType
        {
            get { return null; }
        }

        public override Type ModelType => typeof(LayoutTabViewModel);

        public override string TabHtmlId => "layout-content";

        public override Task RenderTabPane(IHtmlHelper html, ISessionAwareMapper mapper, Webpage webpage)
        {
            return html.RenderPartialAsync("Layout", mapper.Map<LayoutTabViewModel>(webpage));
        }

        public override Task<string> Name(IServiceProvider serviceProvider, Webpage entity)
        {
            return Task.FromResult("Layout");
        }

        public override Task<bool> ShouldShow(IServiceProvider serviceProvider, Webpage entity)
        {
            return Task.FromResult(true);
        }
    }
}