using Microsoft.AspNetCore.Mvc;
using MrCMS.Web.Admin.Infrastructure.BaseControllers;
using MrCMS.Web.Admin.Services.Jobs;

namespace MrCMS.Web.Admin.Controllers;

public class AdHocJobController : MrCMSAdminController
{
    private readonly IAdHocJobAdminService _adminService;

    public AdHocJobController(IAdHocJobAdminService adminService)
    {
        _adminService = adminService;
    }
    
    public IActionResult Index()
    {
        var jobs = _adminService.GetAdHocJobs();

        return View(jobs);
    }
    
    [HttpGet]
    public IActionResult Trigger(string id)
    {
        return View(_adminService.GetAdHocJob(id));
    }

    [HttpPost, ActionName("Trigger")]
    public IActionResult Trigger_POST(string id)
    {
        _adminService.TriggerAdHocJob(id);

        return RedirectToAction("Index");
    }
}