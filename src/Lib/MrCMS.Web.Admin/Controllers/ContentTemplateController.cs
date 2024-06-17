using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MrCMS.Web.Admin.Infrastructure.BaseControllers;
using MrCMS.Web.Admin.Infrastructure.Helpers;
using MrCMS.Web.Admin.Models;
using MrCMS.Web.Admin.Services;

namespace MrCMS.Web.Admin.Controllers;

public class ContentTemplateController : MrCMSAdminController
{
    private readonly IContentTemplateAdminService _contentTemplateAdminService;

    public ContentTemplateController(IContentTemplateAdminService contentTemplateAdminService)
    {
        _contentTemplateAdminService = contentTemplateAdminService;
    }
    
    [HttpGet]
    public async Task<ViewResult> Index(ContentTemplateSearchModel searchModel)
    {
        ViewData["items"] = await _contentTemplateAdminService.SearchAsync(searchModel);
        return View(searchModel);
    }

    [HttpGet]
    public ActionResult Add()
    {
        return View(new AddContentTemplateModel());
    }

    [HttpPost]
    public async Task<IActionResult> Add(AddContentTemplateModel addContentTemplateModel)
    {
        if (!ModelState.IsValid) return View(addContentTemplateModel);

        if (!await _contentTemplateAdminService.IsUniqueName(addContentTemplateModel.Name, null))
        {
            ModelState.AddModelError(string.Empty,$"{addContentTemplateModel.Name} already registered.");
            return View(addContentTemplateModel);
        }
        
        await _contentTemplateAdminService.AddAsync(addContentTemplateModel);
        return RedirectToAction("Index");

    }

    [HttpGet]
    public async Task<ViewResult> Edit(int id)
    {
        var updateContentTemplateModel = await _contentTemplateAdminService.GetUpdateModel(id);
        return View(updateContentTemplateModel);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(UpdateContentTemplateModel model)
    {
        if (!ModelState.IsValid) return View(model);
        
        if (!await _contentTemplateAdminService.IsUniqueName(model.Name, model.Id))
        {
            ModelState.AddModelError(string.Empty,$"{model.Name} already registered.");
            return View(model);
        }
        
        await _contentTemplateAdminService.UpdateAsync(model);
        TempData.AddSuccessMessage($"'{model.Name}' updated");
        return RedirectToAction("Index");

    }

    [HttpGet]
    public async Task<ViewResult> Delete(int id)
    {
        return View(await _contentTemplateAdminService.GetAsync(id));
    }

    [HttpPost]
    [ActionName(nameof(Delete))]
    public async Task<RedirectToActionResult> Delete_POST(int id)
    {
        await _contentTemplateAdminService.DeleteAsync(id);
        return RedirectToAction("Index");
    }

    public async Task<JsonResult> IsUniqueName(string name, int? id)
    {
        return await _contentTemplateAdminService.IsUniqueName(name, id)
            ? Json(true)
            : Json($"{name} already registered.");
    }
}