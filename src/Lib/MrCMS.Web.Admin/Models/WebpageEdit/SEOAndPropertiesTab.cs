using Microsoft.AspNetCore.Mvc.Rendering;
using MrCMS.Entities.Documents.Web;
using MrCMS.Web.Admin.Infrastructure.Models.Tabs;
using System;
using System.Threading.Tasks;
using MrCMS.Mapping;

namespace MrCMS.Web.Admin.Models.WebpageEdit
{
    public class SEOAndPropertiesTab : AdminTab<Webpage>
    {
        public override int Order => 100;

        public override Task<string> Name(IServiceProvider serviceProvider, Webpage entity)
        {
            return Task.FromResult("SEO & Properties");
        }

        public override Task<bool> ShouldShow(IServiceProvider serviceProvider, Webpage entity)
        {
            return Task.FromResult(entity is not Redirect);
        }

        public override Type ParentType => null;

        public override Type ModelType => typeof(SEOTabViewModel);

        public override string TabHtmlId => "edit-seo";

        public override Task RenderTabPane(IHtmlHelper html, ISessionAwareMapper mapper, Webpage webpage)
        {
            return html.RenderPartialAsync("SEO", mapper.Map<SEOTabViewModel>(webpage));
        }
    }
}