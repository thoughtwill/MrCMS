using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MrCMS.Entities.Documents.Web;
using MrCMS.Web.Admin.Infrastructure.BaseControllers;
using MrCMS.Web.Admin.Infrastructure.Helpers;
using MrCMS.Web.Admin.Models.Content;
using MrCMS.Web.Admin.Services;
using MrCMS.Website;

namespace MrCMS.Web.Admin.Controllers;

public class ContentVersionController : MrCMSAdminController
{
    private readonly IContentVersionAdminService _adminService;
    private readonly IGetWebpageForPath _getWebpageForPath;
    
    private const string CopyContentVersionSessionKey = "CopiedContentVersion";

    public ContentVersionController(IContentVersionAdminService adminService,
        IGetWebpageForPath getWebpageForPath)
    {
        _adminService = adminService;
        _getWebpageForPath = getWebpageForPath;
    }

    public ViewResult AddInitial(int webpageId)
    {
        return View(new AddInitialContentVersionModel { WebpageId = webpageId });
    }

    [HttpPost]
    public async Task<ActionResult> AddInitial(AddInitialContentVersionModel model)
    {
        var version = await _adminService.AddInitialContentVersion(model);

        return RedirectToAction("Edit", "Webpage", new { id = model.WebpageId });
    }

    public async Task<IActionResult> EditLatest(int id)
    {
        var versions = await _adminService.GetVersions(id);

        if (versions.Any(x => x.Status == ContentVersionStatus.Draft))
            return RedirectToAction("Edit", new { versions.First(x => x.Status == ContentVersionStatus.Draft).Id });

        if (!versions.Any(x => x.IsLive))
        {
            var version =
                await _adminService.AddInitialContentVersion(new AddInitialContentVersionModel { WebpageId = id });

            return RedirectToAction("Edit", new { version.Id });
        }
        else
        {
            var liveVersion = versions.First(x => x.IsLive);

            var version = await _adminService.AddDraftBasedOnVersion(liveVersion);

            return RedirectToAction("Edit", new { version.Id });
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        var editModel = await _adminService.GetEditModel(id);
        if (editModel == null)
            return NotFound();
        
        return View(editModel);
    }

    public async Task<IActionResult> EditByUrl(string url)
    {
        var path = new Uri(url).LocalPath.TrimStart('/');

        var page = await _getWebpageForPath.GetWebpage(path);
        if (page != null)
        {
            return RedirectToAction("EditLatest", new { page.Id });
        }

        ViewData["PreviewUrl"] = url;
        return View("Edit",null);
    }

    [HttpPost]
    public async Task<RedirectToActionResult> Publish(int id)
    {
        var result = await _adminService.Publish(id);
        if (result.Success)
        {
            TempData.AddSuccessMessage(result.Message);
        }
        else
        {
            TempData.AddErrorMessage(result.Message);
        }

        return result.WebpageId.HasValue
            ? RedirectToAction("Edit", "Webpage", new { id = result.WebpageId })
            : RedirectToAction("Index", "Webpage");
    }

    [HttpPost]
    public async Task<RedirectToActionResult> Delete(int id)
    {
        var result = await _adminService.Delete(id);
        if (result.Success)
        {
            TempData.AddSuccessMessage(result.Message);
        }
        else
        {
            TempData.AddErrorMessage(result.Message);
        }

        return result.WebpageId.HasValue
            ? RedirectToAction("Edit", "Webpage", new { id = result.WebpageId })
            : RedirectToAction("Index", "Webpage");
    }

    public async Task<IActionResult> Blocks(int id, Guid? selected)
    {
        var editModel = await _adminService.GetEditModel(id);
        if (editModel == null)
            return NotFound();
        
        ViewData["selected"] = selected;
        return PartialView(editModel);
    }
    
    public async Task<ViewResult> Copy(int id)
    {
        HttpContext.Session.SetInt32(CopyContentVersionSessionKey, id);
        return View("Edit", await _adminService.GetEditModel(id));
    }

    public async Task<ViewResult> Paste(int id)
    {
        var copyContentVersionId = HttpContext.Session.GetInt32(CopyContentVersionSessionKey);
        if (copyContentVersionId.HasValue)
        {
            await _adminService.CopyContentVersion(copyContentVersionId.Value, id);
        }
        return View("Edit", await _adminService.GetEditModel(id));
    }
}