using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MrCMS.Web.Admin.Infrastructure.BaseControllers;
using MrCMS.Web.Admin.Infrastructure.Helpers;
using MrCMS.Web.Admin.Models;
using MrCMS.Web.Admin.Services;

namespace MrCMS.Web.Admin.Controllers
{
    public class RedirectsController : MrCMSAdminController
    {
        private readonly IRedirectsAdminService _adminService;

        public RedirectsController(IRedirectsAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet]
        public async Task<ViewResult> Index(RedirectsSearchQuery searchQuery)
        {
            ViewData["redirects"] = await _adminService.SearchAll(searchQuery);
            return View(searchQuery);
        }

        [HttpGet]
        public async Task<ViewResult> Known404s(Known404sSearchQuery searchQuery)
        {
            ViewData["404s"] = await _adminService.SearchKnown404s(searchQuery);
            return View(searchQuery);
        }

        [HttpGet]
        public async Task<IActionResult> MarkAsGone(int id)
        {
            var model = await _adminService.GetUrlHistory(id);
            return View(model);
        }

        [HttpPost, ActionName(nameof(MarkAsGone))]
        public async Task<IActionResult> MarkAsGone_POST(int id)
        {
            await _adminService.MarkAsGone(id);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> MarkAsIgnored(int id)
        {
            var model = await _adminService.GetUrlHistory(id);
            return View(model);
        }

        [HttpPost, ActionName(nameof(MarkAsIgnored))]
        public async Task<IActionResult> MarkAsIgnored_POST(int id)
        {
            await _adminService.MarkAsIgnored(id);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Reset(int id)
        {
            var model = await _adminService.GetUrlHistory(id);
            return View(model);
        }

        [HttpPost, ActionName(nameof(Reset))]
        public async Task<IActionResult> Reset_POST(int id)
        {
            await _adminService.Reset(id);
            return RedirectToAction("Index");
        }
        
        [HttpGet]
        public async Task<IActionResult> Remove(int id)
        {
            var model = await _adminService.GetUrlHistory(id);
            return View(model);
        }

        [HttpPost, ActionName(nameof(Remove))]
        public async Task<IActionResult> Remove_POST(int id)
        {
            await _adminService.Remove(id);
            return RedirectToAction("Index");
        }
        
        
        [HttpGet]
        public async Task<IActionResult> SetRedirectUrl(int id)
        {
            var model = await _adminService.GetSetRedirectUrlModel(id);
            return View(model);
        }

        [HttpPost, ActionName(nameof(SetRedirectUrl))]
        public async Task<IActionResult> SetRedirectUrl(SetRedirectUrlModel model)
        {
            await _adminService.SetRedirectUrl(model);
            return RedirectToAction("Index");
        }
        
        [HttpGet]
        public async Task<IActionResult> SetRedirectPage(int id)
        {
            var model = await _adminService.GetSetRedirectPageModel(id);
            return View(model);
        }

        [HttpPost, ActionName(nameof(SetRedirectPage))]
        public async Task<IActionResult> SetRedirectPage(SetRedirectPageModel model)
        {
            await _adminService.SetRedirectPage(model);
            return RedirectToAction("Index");
        }


        public IActionResult CheckRedirectUrl(string url)
        {
            // if it's absolute, fine
            if(Uri.TryCreate(url, UriKind.Absolute, out var result))
                return Json(true);
            // otherwise check that it's relative to the route
            if (url.StartsWith("/"))
                return Json(true);
            return Json("Internal Url must start with '/'");
        }
        
        [HttpPost]
        public async Task<IActionResult> ImportRedirects()
        {
            const string csvContentType = "text/csv";
            var file = Request.Form.Files.FirstOrDefault(f => f.ContentType == "text/csv");
            if (file?.ContentType != csvContentType)
            {
                TempData.AddErrorMessage("Please upload a .csv file");
                return RedirectToAction("Index", "Redirects");
            }

            var result = await _adminService.ImportRedirects(file.OpenReadStream());

            if (result.Errors.Any())
            {
                var errorBytes = await MrCMS.Helpers.CsvHelper.GenerateCsvBytes(result.Errors, Encoding.UTF8);
                var cd = new System.Net.Mime.ContentDisposition
                {
                    FileName = "Error list.csv",
                    Inline = true
                };
                Response.Headers.Add("Content-Disposition", cd.ToString());
                return File(errorBytes, csvContentType);
            }

            TempData.AddSuccessMessage($"{result.ImportedCount} Redirects data successfully imported.");
            return RedirectToAction("Index", "Redirects");
        }
    }
}