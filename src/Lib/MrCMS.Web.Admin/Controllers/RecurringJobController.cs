using Microsoft.AspNetCore.Mvc;
using MrCMS.Web.Admin.Infrastructure.BaseControllers;
using MrCMS.Web.Admin.Models.Jobs;
using MrCMS.Web.Admin.Services.Jobs;

namespace MrCMS.Web.Admin.Controllers;

public class RecurringJobController : MrCMSAdminController
{
    private readonly IRecurringJobAdminService _adminService;

    public RecurringJobController(IRecurringJobAdminService adminService)
    {
        _adminService = adminService;
    }

    public IActionResult Index()
    {
        var jobs = _adminService.GetRecurringJobs();

        return View(jobs);
    }

    [HttpGet]
    public IActionResult Remove(string id)
    {
        return View(_adminService.GetRecurringJobSetupModel(id));
    }

    [HttpPost, ActionName("Remove")]
    public IActionResult Remove_POST(string id)
    {
        _adminService.RemoveRecurringJob(id);

        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult Trigger(string id)
    {
        return View(_adminService.GetRecurringJobSetupModel(id));
    }

    [HttpPost, ActionName("Trigger")]
    public IActionResult Trigger_POST(string id)
    {
        _adminService.TriggerRecurringJob(id);

        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult Setup(string id)
    {
        return View(_adminService.GetRecurringJobSetupModel(id));
    }

    [HttpPost]
    public IActionResult Setup(SetupRecurringJobModel model)
    {
        _adminService.SetupRecurringJob(model);

        return RedirectToAction("Index");
    }
}