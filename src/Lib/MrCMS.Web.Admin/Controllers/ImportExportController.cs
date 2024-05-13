using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MrCMS.Helpers;
using MrCMS.Models;
using MrCMS.Services;
using MrCMS.Services.ImportExport;
using MrCMS.Web.Admin.Infrastructure.BaseControllers;

namespace MrCMS.Web.Admin.Controllers
{
    public class ImportExportController : MrCMSAdminController
    {
        private readonly IImportExportManager _importExportManager;
        private readonly ICurrentSiteLocator _currentSiteLocator;
        private readonly ILogger<ImportExportController> _logger;

        public ImportExportController(IImportExportManager importExportManager, ICurrentSiteLocator currentSiteLocator,
            ILogger<ImportExportController> logger)
        {
            _importExportManager = importExportManager;
            _currentSiteLocator = currentSiteLocator;
            _logger = logger;
        }

        [HttpGet]
        public ActionResult Webpages()
        {
            //if (TempData.ContainsKey("messages"))
            ViewBag.Messages = TempData.Get<ImportWebpagesResult>("messages");
            if (TempData.ContainsKey("import-status"))
                ViewBag.ImportStatus = TempData["import-status"];
            if (TempData.ContainsKey("export-status"))
                ViewBag.ExportStatus = TempData["export-status"];
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> ExportWebpages()
        {
            try
            {
                byte[] file = await _importExportManager.ExportWebpagesToExcel();
                TempData["export-status"] = "Documents successfully exported.";
                return File(file, ImportExportManager.XlsxContentType,
                    "MrCMS-Documents-" + DateTime.UtcNow + ".xlsx");
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error exporting documents");
                TempData["export-status"] =
                    "Documents exporting has failed. Please try again and contact system administration if error continues to appear.";
                return RedirectToAction("Webpages");
            }
        }

        [HttpPost]
        public async Task<ActionResult> ImportWebpages(IFormFile file)
        {
            if (file is { Length: > 0, ContentType: ImportExportManager.XlsxContentType })
                TempData.Set(
                    await _importExportManager.ImportWebpagesFromExcel(
                        _currentSiteLocator.GetCurrentSite().Id,
                        file.OpenReadStream()),
                    "messages");
            else
                TempData["import-status"] = "Please choose non-empty Excel (.xslx) file before uploading.";
            return RedirectToAction("Webpages");
        }

        [HttpPost]
        public async Task<ActionResult> ExportWebpagesToEmail(ExportWebpagesModel model)
        {
            try
            {
                await _importExportManager.ExportWebpagesToEmail(model);
                TempData["export-status"] = "Documents successfully exported.";
            }
            catch
            {
                TempData["export-status"] =
                    "Documents exporting has failed. Please try again and contact system administration if error continues to appear.";
            }

            return RedirectToAction("Webpages");
        }
    }
}