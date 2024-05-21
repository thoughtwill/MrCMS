using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MrCMS.ACL.Rules;
using MrCMS.Web.Admin.Infrastructure.BaseControllers;
using MrCMS.Web.Admin.Models;
using MrCMS.Web.Admin.Services;
using MrCMS.Website;

namespace MrCMS.Web.Admin.Controllers
{
    [Acl(typeof(AdminBarACL), AdminBarACL.Show, ReturnEmptyResult = true)]
    public class InPageAdminController : MrCMSAdminController
    {
        private readonly IInPageAdminService _inPageAdminService;
        private readonly IWebpageAdminService _webpageUiService;
        private readonly StringResourceAdminService _stringResourceAdminService;

        public InPageAdminController(IInPageAdminService inPageAdminService, IWebpageAdminService webpageUiService,
            StringResourceAdminService stringResourceAdminService)
        {
            _inPageAdminService = inPageAdminService;
            _webpageUiService = webpageUiService;
            _stringResourceAdminService = stringResourceAdminService;
        }

        public async Task<ActionResult> InPageEditor(int id)
        {
            return PartialView("InPageEditor", await _webpageUiService.GetWebpage(id));
        }

        [HttpPost]
        public async Task<JsonResult> SaveContent([FromBody] UpdatePropertyData updatePropertyData)
        {
            return Json(await _inPageAdminService.SaveContent(updatePropertyData));
        }

        public async Task<PartialViewResult> GetUnformattedContent(GetPropertyData getPropertyData)
        {
            return PartialView(await _inPageAdminService.GetContent(getPropertyData));
        }

        public async Task<PartialViewResult> GetFormattedContent(GetPropertyData getPropertyData)
        {
            return PartialView(await _inPageAdminService.GetContent(getPropertyData));
        }

        [HttpPost]
        public async Task<JsonResult> SaveStringResource([FromBody] StringResourceInlineUpdateModel model)
        {
            return Json(await _stringResourceAdminService.Update(model));
        }

        public async Task<PartialViewResult> GetUnformattedStringResource(string key)
        {
            return PartialView(await _stringResourceAdminService.GetResource(key));
        }

        [HttpPost]
        public async Task<PartialViewResult> GetFormattedStringResource(string key,
            [FromBody] List<StringResourceReplacementModel> replacements)
        {
            var stringResource = await _stringResourceAdminService.GetResource(key);

            var output = stringResource.Value;
    
            if (replacements?.Any() ?? false)
            {
                output = replacements.Aggregate(stringResource.Value,
                    (current, replacement) => current.Replace($"{{{replacement.Key}}}", replacement.Value));
            }

            return PartialView((object)output);
        }
    }
}