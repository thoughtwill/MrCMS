﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MrCMS.Services.FileMigration;
using MrCMS.Web.Apps.Admin.Helpers;
using MrCMS.Website.Controllers;

namespace MrCMS.Web.Apps.Admin.Controllers
{
    public class FileMigrationController : MrCMSAdminController
    {
        private readonly IFileMigrationService _fileMigrationService;

        public FileMigrationController(IFileMigrationService fileMigrationService)
        {
            _fileMigrationService = fileMigrationService;
        }

        public PartialViewResult Show()
        {
            return PartialView();
        }

        [HttpPost]
        public async Task<ActionResult> Migrate()
        {
            FileMigrationResult result = await _fileMigrationService.MigrateFiles();

            if (result.MigrationRequired)
                TempData.SuccessMessages().Add(result.Message);
            else
                TempData.InfoMessages().Add(result.Message);

            return RedirectToAction("FileSystem", "Settings");
        }
    }
}