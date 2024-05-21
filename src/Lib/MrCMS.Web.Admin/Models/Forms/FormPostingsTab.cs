using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using MrCMS.Entities.Documents.Web;
using MrCMS.Mapping;
using MrCMS.Web.Admin.Infrastructure.Models.Tabs;
using MrCMS.Web.Admin.Services;

namespace MrCMS.Web.Admin.Models.Forms
{
    public class FormPostingsTab : AdminTab<Form>
    {
        public override int Order => 200;

        public override async Task<string> Name(IServiceProvider serviceProvider, Form entity)
        {
            var postingsModel = await serviceProvider.GetRequiredService<IFormAdminService>()
                .GetFormPostings(entity, 1, string.Empty);
            return $"Entries ({postingsModel.Items.Count})";
        }

        public override Task<bool> ShouldShow(IServiceProvider serviceProvider, Form entity)
        {
            return Task.FromResult(true);
        }

        public override Type ParentType => null;

        public override Type ModelType => null;

        public override string TabHtmlId => "form-postings-tab";

        public override Task RenderTabPane(IHtmlHelper html, ISessionAwareMapper mapper, Form form)
        {
            return html.RenderPartialAsync("FormPostings", form);
        }
    }
}