using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MrCMS.ACL.Rules;
using MrCMS.Models;
using MrCMS.Services;
using MrCMS.Services.Resources;
using MrCMS.Settings;
using MrCMS.Web.Admin.ModelBinders;
using MrCMS.Web.Admin.Infrastructure.BaseControllers;
using MrCMS.Web.Admin.Infrastructure.Helpers;
using MrCMS.Website;

namespace MrCMS.Web.Admin.Controllers
{
    public class SystemSettingsController : MrCMSAdminController
    {
        private readonly ISystemConfigurationProvider _configurationProvider;
        private readonly ITestSmtpSettings _testSmtpSettings;
        private readonly IStringResourceProvider _resourceProvider;

        public SystemSettingsController(ISystemConfigurationProvider configurationProvider,
            ITestSmtpSettings testSmtpSettings, IStringResourceProvider resourceProvider)
        {
            _configurationProvider = configurationProvider;
            _testSmtpSettings = testSmtpSettings;
            _resourceProvider = resourceProvider;
        }

        [HttpGet]
        [Acl(typeof(SystemSettingsACL), SystemSettingsACL.View)]
        public ViewResult Index()
        {
            var settings = _configurationProvider.GetAllSystemSettings().FindAll(arg => arg.RenderInSettings);
            return View(settings);
        }

        [HttpPost]
        [ActionName("Index")]
        [Acl(typeof(SystemSettingsACL), SystemSettingsACL.Save)]
        public async Task<RedirectToActionResult> Index_Post(
            [ModelBinder(typeof(SystemSettingsModelBinder))]
            List<SystemSettingsBase> settings)
        {
            foreach (var setting in settings)
            {
                await _configurationProvider.SaveSettings(setting);
            }

            TempData["settings-saved"] = true;
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Acl(typeof(SystemSettingsACL), SystemSettingsACL.View)]
        public ViewResult Mail()
        {
            var mailSettings = _configurationProvider.GetSystemSettings<MailSettings>();
            return View(mailSettings);
        }

        [HttpPost]
        [Acl(typeof(SystemSettingsACL), SystemSettingsACL.Save)]
        public async Task<RedirectToActionResult> Mail(MailSettings settings)
        {
            await _configurationProvider.SaveSettings(settings);
            TempData.AddSuccessMessage("Mail settings saved.");
            return RedirectToAction("Mail");
        }

        [HttpPost]
        [Acl(typeof(SystemSettingsACL), SystemSettingsACL.Save)]
        public async Task<RedirectToActionResult> TestMailSettings(TestEmailInfo info)
        {
            var result =
                await _testSmtpSettings.TestSettings(_configurationProvider.GetSystemSettings<MailSettings>(), info);
            if (result)
                TempData.AddSuccessMessage(await _resourceProvider.GetValue("Admin - Test email - Success",
                    options => options.SetDefaultValue("Email sent.")));
            else
                TempData.AddErrorMessage(await _resourceProvider.GetValue("Admin - Test email - Failure",
                    options => options.SetDefaultValue("An error occurred, check the log for details.")));
            return RedirectToAction("Mail");
        }
    }
}