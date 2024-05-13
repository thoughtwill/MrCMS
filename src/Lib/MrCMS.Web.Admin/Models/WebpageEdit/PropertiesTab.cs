using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc.Rendering;
using MrCMS.Entities.Documents.Web;
using MrCMS.Mapping;
using MrCMS.Web.Admin.Infrastructure.Models.Tabs;

namespace MrCMS.Web.Admin.Models.WebpageEdit
{
    public class PropertiesTab : AdminTab<Webpage>
    {
        public override int Order => 0;

        public override Task<string> Name(IServiceProvider serviceProvider, Webpage entity)
        {
            return Task.FromResult("Properties");
        }

        public override Task<bool> ShouldShow(IServiceProvider serviceProvider, Webpage entity)
        {
            return Task.FromResult(true);
        }

        public override Type ParentType => null;

        public override Type ModelType => typeof(WebpagePropertiesTabViewModel);

        public override string TabHtmlId => "edit-content";

        public override Task RenderTabPane(IHtmlHelper html, ISessionAwareMapper mapper, Webpage webpage)
        {
            return html.RenderPartialAsync("Properties", mapper.Map<WebpagePropertiesTabViewModel>(webpage));
        }
    }
}