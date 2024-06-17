using Microsoft.AspNetCore.Mvc;
using MrCMS.Helpers;
using MrCMS.Services;
using System;
using System.Threading.Tasks;
using MrCMS.ContentTemplates.Widgets;
using MrCMS.Web.Admin.Infrastructure.BaseControllers;
using MrCMS.Web.Admin.Models;
using MrCMS.Web.Admin.Models.Widgets;
using MrCMS.Web.Admin.Services;

namespace MrCMS.Web.Admin.Controllers
{
    public class WidgetController : MrCMSAdminController
    {
        private readonly IWidgetAdminService _widgetService;
        private readonly ISetWidgetAdminViewData _setAdminViewData;
        private readonly IModelBindingHelperAdapter _modelBindingHelperAdapter;

        public WidgetController(IWidgetAdminService widgetService, ISetWidgetAdminViewData setAdminViewData,
            IModelBindingHelperAdapter modelBindingHelperAdapter)
        {
            _widgetService = widgetService;
            _setAdminViewData = setAdminViewData;
            _modelBindingHelperAdapter = modelBindingHelperAdapter;
        }

        [HttpGet]
        public PartialViewResult Add(int id, string returnUrl = null)
        {
            ViewData["return-url"] = returnUrl;
            var model = new AddWidgetModel
            {
                LayoutAreaId = id,
            };
            return PartialView(model);
        }

        [HttpPost]
        [ActionName("Add")]
        public async Task<IActionResult> Add_POST(AddWidgetModel model, string returnUrl = null)
        {
            if (int.TryParse(model.WidgetType, out var contentTemplateId))
            {
                // If parsing is successful, this mean it is content template (We should later use radio button for this)
                model.WidgetType = typeof(ContentTemplateWidget).FullName;
            }

            var additionalPropertyModel = _widgetService.GetAdditionalPropertyModel(model.WidgetType);
            if (additionalPropertyModel != null && contentTemplateId == 0)
            {
                await _modelBindingHelperAdapter.TryUpdateModelAsync(this, additionalPropertyModel,
                    additionalPropertyModel.GetType(), string.Empty);
            }
            else if (contentTemplateId > 0)
            {
                additionalPropertyModel = new ContentTemplateWidgetAddModel()
                {
                    ContentTemplateId = contentTemplateId
                };
            }

            var widget = await _widgetService.AddWidget(model, additionalPropertyModel);

            int layoutAreaId = 0;

            if (widget.LayoutArea != null)
            {
                layoutAreaId = widget.LayoutArea.Id;
            }

            return !string.IsNullOrWhiteSpace(returnUrl) &&
                   !returnUrl.Contains("widget/edit/", StringComparison.OrdinalIgnoreCase)
                ? Redirect(returnUrl)
                : RedirectToAction("Edit", "LayoutArea", new { id = layoutAreaId });
        }

        [HttpGet]
        [ActionName("Edit")]
        public async Task<ViewResult> Edit_Get(int id, string returnUrl = null)
        {
            var editModel = await _widgetService.GetEditModel(id);
            var widget = await _widgetService.GetWidget(id);
            await _setAdminViewData.SetViewData(ViewData, widget);
            ViewData["widget"] = widget;

            ViewData["return-url"] = returnUrl;

            return View(editModel);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(UpdateWidgetModel model, string returnUrl = null)
        {
            var additionalPropertyModel = await _widgetService.GetUpdateAdditionalPropertyModel(model.Id);
            if (additionalPropertyModel != null)
            {
                await _modelBindingHelperAdapter.TryUpdateModelAsync(this, additionalPropertyModel,
                    additionalPropertyModel.GetType(), string.Empty);
            }

            var widget = await _widgetService.UpdateWidget(model, additionalPropertyModel);

            return string.IsNullOrWhiteSpace(returnUrl)
                ? (ActionResult)RedirectToAction("Edit", "LayoutArea", new { id = widget.LayoutArea.Id })
                : Redirect(returnUrl);
        }

        [HttpGet]
        [ActionName("Delete")]
        public async Task<ActionResult> Delete_Get(int id, string returnUrl = "")
        {
            if (string.IsNullOrEmpty(returnUrl))
            {
                ViewData["return-url"] = Request.Referer();
            }
            else
            {
                ViewData["return-url"] = returnUrl;
            }

            return PartialView(await _widgetService.GetEditModel(id));
        }

        [HttpPost]
        public async Task<ActionResult> Delete(int id, string returnUrl)
        {
            var widget = await _widgetService.DeleteWidget(id);

            int layoutAreaId = 0;

            if (widget.LayoutArea != null)
            {
                layoutAreaId = widget.LayoutArea.Id;
            }

            return !string.IsNullOrWhiteSpace(returnUrl) &&
                   !returnUrl.Contains("widget/edit/", StringComparison.OrdinalIgnoreCase)
                ? (ActionResult)Redirect(returnUrl)
                : RedirectToAction("Edit", "LayoutArea", new { id = layoutAreaId });
        }

        [HttpGet]
        public async Task<ActionResult> AddProperties(string type)
        {
            var model = _widgetService.GetAdditionalPropertyModel(type);
            if (model != null)
            {
                await _setAdminViewData.SetViewDataForAdd(ViewData, type);
                ViewData["type"] = TypeHelper.GetTypeByName(type);
                return PartialView(model);
            }

            return new EmptyResult();
        }
    }
}