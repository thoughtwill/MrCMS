using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MrCMS.ACL.Rules;
using MrCMS.AI.Settings;
using MrCMS.Web.Admin.Infrastructure.BaseControllers;
using MrCMS.Web.Admin.ModelBinders;
using MrCMS.Website;

namespace MrCMS.Web.Admin.Controllers
{
    public class AiSettingsController : MrCMSAdminController
    {
        private readonly IAiConfigurationProvider _configurationProvider;
        private readonly IServiceProvider _serviceProvider;

        public AiSettingsController(IAiConfigurationProvider configurationProvider, IServiceProvider serviceProvider)
        {
            _configurationProvider = configurationProvider;
            _serviceProvider = serviceProvider;
        }

        [HttpGet]
        [Acl(typeof(AiSiteSettingsACL), AiSiteSettingsACL.View)]
        public ViewResult Index()
        {
            var settings = _configurationProvider.GetAllSettings().FindAll(arg => arg.RenderInSettings);
            settings.ForEach(@base => @base.SetViewData(_serviceProvider, ViewData));
            return View(settings);
        }

        [HttpPost]
        [ActionName("Index")]
        [Acl(typeof(AiSiteSettingsACL), AiSiteSettingsACL.Save)]
        public async Task<RedirectToActionResult> Index_Post(
            [ModelBinder(typeof(AiSettingsModelBinder))]
            List<AiSettingsBase> settings)
        {
            foreach (var setting in settings)
            {
                await _configurationProvider.SaveSettings(setting);
            }
            TempData["settings-saved"] = true;
            return RedirectToAction("Index");
        }
    }
}