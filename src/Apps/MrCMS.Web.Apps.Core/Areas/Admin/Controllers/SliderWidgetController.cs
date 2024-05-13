using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MrCMS.Services;
using MrCMS.Web.Admin.Infrastructure.BaseControllers;
using MrCMS.Web.Admin.Infrastructure.Helpers;
using MrCMS.Web.Apps.Core;
using MrCMS.Web.Apps.Core.Models.Widgets;
using MrCMS.Web.Apps.Core.Widgets;
using Newtonsoft.Json;

namespace MrCMS.Web.Apps.ElliottBrothers.Areas.Admin.Controllers;

public class SliderWidgetController : MrCMSAppAdminController<MrCMSCoreApp>
{
    private readonly IWidgetService _widgetService;

    public SliderWidgetController(IWidgetService widgetService)
    {
        _widgetService = widgetService;
    }
    

    [HttpGet]
    public async Task<IActionResult> Edit(int widgetId, int slideIndex)
    {
        var widget = await _widgetService.GetWidget<SliderWidget>(widgetId);

        var slides = widget?.Slides;
        if (slideIndex >= slides?.Count)
        {
            TempData.AddErrorMessage($"Slide not found");
            return Redirect($"/Admin/Widget/Edit/{widgetId}");
        }
        var model = widget?.Slides[slideIndex];

        ViewData["widgetId"] = widgetId;
        ViewData["slideIndex"] = slideIndex;
        return View(model);
    }

    [HttpPost]
    public async Task<RedirectResult> Edit(int widgetId, int slideIndex, SlideViewModel model)
    {
        if (ModelState.IsValid)
        {
            var widget = await _widgetService.GetWidget<SliderWidget>(widgetId);

            if (widget == null)
            {
                TempData.AddErrorMessage($"Widget not found");
                return Redirect($"/Admin/Widget/Edit/{widgetId}");
            }

            var slides = widget.Slides ?? new List<SlideViewModel>();
            
            if (slideIndex >= slides.Count)
            {
                TempData.AddErrorMessage($"Slide not found");
                return Redirect($"/Admin/Widget/Edit/{widgetId}");
            }
            
            var slide = slides[slideIndex];
            slide.Caption = model.Caption;
            slide.Image = model.Image;
            slide.Link = model.Link;
            slide.SmallImage = model.SmallImage;

            widget.SlideList = JsonConvert.SerializeObject(slides);
            await _widgetService.SaveWidget(widget);
            TempData.AddSuccessMessage("Slide updated successfully");
        }
        else
        {
            var modelErrors = ModelState.Values.Where(v => v.Errors.Any()).SelectMany(v => v.Errors);
            foreach (var error in modelErrors)
            {
                TempData.AddErrorMessage($"{error.ErrorMessage}");
            }
        }
        

        return Redirect($"/Admin/Widget/Edit/{widgetId}");
    }
}