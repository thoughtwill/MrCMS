using System;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.AspNetCore.Mvc;
using MrCMS.AI;
using MrCMS.AI.Models;
using MrCMS.Web.Admin.Infrastructure.BaseControllers;
using MrCMS.Web.Admin.Models;
using MrCMS.Web.Admin.Models.Prompts;
using MrCMS.Web.Admin.Services;

namespace MrCMS.Web.Admin.Controllers;

public class PromptController : MrCMSAdminController
{
    private readonly IPromptAdminService _promptAdminService;

    public PromptController(IPromptAdminService promptAdminService)
    {
        _promptAdminService = promptAdminService;
    }

    public async Task<IActionResult> Index(PromptSearchModel search)
    {
        ViewData["items"] = await _promptAdminService.GetPrompts(search);

        return View(search);
    }

    public IActionResult Add(PromptType type)
    {
        var model = new AddPromptModel
        {
            Type = type
        };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Add(AddPromptModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (!await _promptAdminService.IsUniqueName(model.Name, model.Type, null))
        {
            ModelState.AddModelError(string.Empty, $"{model.Name} already registered.");
            return View(model);
        }

        await _promptAdminService.AddPrompt(model);
        return RedirectToAction("Index", new { model.Type });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var model = await _promptAdminService.GetEditModel(id);
        return View(model);
    }

    [HttpPost, ActionName("Edit")]
    public async Task<IActionResult> Edit_Post(UpdatePromptModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (!await _promptAdminService.IsUniqueName(model.Name, model.Type, model.Id))
        {
            ModelState.AddModelError(string.Empty, $"{model.Name} already registered.");
            return View(model);
        }

        await _promptAdminService.UpdatePrompt(model);
        return RedirectToAction("Index", new { model.Type });
    }

    public async Task<ViewResult> Delete(int id)
    {
        return View(await _promptAdminService.GetEditModel(id));
    }

    [HttpPost]
    [ActionName(nameof(Delete))]
    public async Task<RedirectToActionResult> Delete_POST(int id)
    {
        var type = (await _promptAdminService.GetEditModel(id)).Type;
        await _promptAdminService.DeletePrompt(id);
        return RedirectToAction("Index", new { type });
    }

    public async Task<JsonResult> IsUniqueName(string name, PromptType type, int? id)
    {
        return await _promptAdminService.IsUniqueName(name, type, id)
            ? Json(true)
            : Json($"{name} already registered.");
    }
}