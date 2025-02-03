using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using MrCMS.Jobs;
using MrCMS.Web.Admin.Infrastructure.BaseControllers;
using MrCMS.Web.Admin.Infrastructure.Helpers;
using MrCMS.Web.Admin.Models;
using MrCMS.Web.Admin.Services;

namespace MrCMS.Web.Admin.Controllers
{
    public class TextSearchController : MrCMSAdminController
    {
        private readonly IQuickSearcher _quickSearcher;

        public TextSearchController(IQuickSearcher quickSearcher)
        {
            _quickSearcher = quickSearcher;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Refresh()
        {
            //add hangfire job to refresh index
            BackgroundJob.Enqueue<IRefreshTextSearchIndexTask>(x => x.Refresh());

            TempData.AddSuccessMessage("Index refresh started");

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<JsonResult> QuickSearch(QuickSearchParams searchParams)
        {
            return Json(await _quickSearcher.QuickSearch(searchParams));
        }
    }
}