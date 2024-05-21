using System;
using Microsoft.AspNetCore.Mvc;
using MrCMS.Entities.Documents.Web;
using MrCMS.Helpers;
using MrCMS.Models;
using MrCMS.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MrCMS.Web.Admin.Infrastructure.BaseControllers;
using MrCMS.Web.Admin.Infrastructure.Helpers;
using MrCMS.Web.Admin.Models;
using MrCMS.Web.Admin.Services;

namespace MrCMS.Web.Admin.Controllers
{
    public class WebpageController : MrCMSAdminController
    {
        private readonly IWebpageAdminService _webpageAdminService;
        private readonly IWebpageBaseViewDataService _webpageBaseViewDataService;
        private readonly ISetWebpageAdminViewData _setWebpageAdminViewData;
        private readonly IUrlValidationService _urlValidationService;
        private readonly IModelBindingHelperAdapter _modelBindingHelperAdapter;
        private readonly IWebpageVersionsAdminService _webpageVersionsAdminService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ICurrentSiteLocator _currentSiteLocator;

        public WebpageController(IWebpageAdminService webpageAdminService,
            IWebpageBaseViewDataService webpageBaseViewDataService,
            ISetWebpageAdminViewData setWebpageAdminViewData,
            IUrlValidationService urlValidationService,
            IModelBindingHelperAdapter modelBindingHelperAdapter,
            IWebpageVersionsAdminService webpageVersionsAdminService,
            IServiceProvider serviceProvider,
            ICurrentSiteLocator currentSiteLocator)
        {
            _webpageAdminService = webpageAdminService;
            _webpageBaseViewDataService = webpageBaseViewDataService;
            _setWebpageAdminViewData = setWebpageAdminViewData;
            _urlValidationService = urlValidationService;
            _modelBindingHelperAdapter = modelBindingHelperAdapter;
            _webpageVersionsAdminService = webpageVersionsAdminService;
            _serviceProvider = serviceProvider;
            _currentSiteLocator = currentSiteLocator;
        }

        public ViewResult Index()
        {
            return View();
        }


        [HttpGet]
        [ActionName("Add")]
        public async Task<ViewResult> Add_Get(int? id)
        {
            //Build list 
            var model = _webpageAdminService.GetAddModel(id);

            Webpage parent = await _webpageAdminService.GetWebpage(id);
            await _webpageBaseViewDataService.SetAddPageViewData(ViewData, parent);

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Add(AddWebpageModel model)
        {
            if (!await _urlValidationService.UrlIsValidForWebpage(_currentSiteLocator.GetCurrentSite().Id,
                    model.UrlSegment, null))
            {
                Webpage parent = await _webpageAdminService.GetWebpage(model.ParentId);
                await _webpageBaseViewDataService.SetAddPageViewData(ViewData, parent);
                return View(model);
            }

            var additionalPropertyModel = _webpageAdminService.GetAdditionalPropertyModel(model.WebpageType);
            if (additionalPropertyModel != null)
            {
                await _modelBindingHelperAdapter.TryUpdateModelAsync(this, additionalPropertyModel,
                    additionalPropertyModel.GetType(), string.Empty);
            }

            var webpage = await _webpageAdminService.Add(model, additionalPropertyModel);
            TempData.AddSuccessMessage($"{webpage.Name} successfully added");
            return RedirectToAction("Edit", new { id = webpage.Id });
        }

        [HttpGet]
        [ActionName("Edit")]
        public async Task<ViewResult> Edit_Get(int id)
        {
            var webpage = await _webpageAdminService.GetWebpage(id);
            await _webpageBaseViewDataService.SetEditPageViewData(ViewData, webpage);
            await _setWebpageAdminViewData.SetViewData(ViewData, webpage);
            return View(webpage);
        }

        [HttpPost]
        public async Task<RedirectToActionResult> Edit(UpdateWebpageViewModel model)
        {
            var result = await _webpageAdminService.Update(model);
            TempData.AddSuccessMessage($"{result.Name} successfully saved");
            return RedirectToAction("Edit", new { id = result.Id });
        }

        [HttpGet]
        [ActionName("Delete")]
        public async Task<ActionResult> Delete_Get(int id)
        {
            return PartialView(await _webpageAdminService.GetWebpage(id));
        }

        [HttpPost]
        public async Task<RedirectToActionResult> Delete(int id)
        {
            var webpage = await _webpageAdminService.Delete(id);
            TempData.AddInfoMessage($"{webpage.Name} deleted");
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<ActionResult> Sort(int? id)
        {
            var sortItems = await _webpageAdminService.GetSortItems(id);

            return View(sortItems);
        }

        [HttpPost]
        public async Task<ActionResult> Sort(List<SortItem> items, int? id)
        {
            await _webpageAdminService.SetOrders(items);
            if (!id.HasValue) // top level sort, need to reset homepage
            {
                await _serviceProvider.GetRequiredService<ISetHomepage>().Set();
            }

            return RedirectToAction("Sort", new { id });
        }

        [HttpGet]
        public async Task<RedirectToActionResult> PublishNow(int id)
        {
            await _webpageAdminService.PublishNow(id);

            return RedirectToAction("Edit", new { id });
        }

        [HttpGet]
        public async Task<RedirectToActionResult> Unpublish(int id)
        {
            await _webpageAdminService.Unpublish(id);

            return RedirectToAction("Edit", new { id });
        }

        public async Task<ActionResult> ViewChanges(int id)
        {
            var webpageVersion = await _webpageVersionsAdminService.GetWebpageVersion(id);
            if (webpageVersion == null)
            {
                return RedirectToAction("Index");
            }

            return PartialView(webpageVersion);
        }

        /// <summary>
        ///     Finds out if the URL entered is valid for a webpage
        /// </summary>
        /// <param name="urlSegment">The URL Segment entered</param>
        /// <param name="id">The Id of the current document if it is set</param>
        /// <returns></returns>
        public async Task<ActionResult> ValidateUrlIsAllowed(string urlSegment, int? id)
        {
            return !await _urlValidationService.UrlIsValidForWebpage(_currentSiteLocator.GetCurrentSite().Id, urlSegment,
                id)
                ? Json("Please choose a different URL as this one is already used.")
                : Json(true);
        }

        /// <summary>
        ///     Returns server date used for publishing (can't use JS date as can be out compared to server date)
        /// </summary>
        /// <returns>Date</returns>
        public string GetServerDate()
        {
            return _webpageAdminService.GetServerDate();
        }

        [HttpGet]
        public async Task<ActionResult> AddProperties(string type)
        {
            var model = _webpageAdminService.GetAdditionalPropertyModel(type);
            if (model != null)
            {
                await _setWebpageAdminViewData.SetViewDataForAdd(ViewData, type);
                ViewData["type"] = TypeHelper.GetTypeByName(type);
                return PartialView(model);
            }

            return new EmptyResult();
        }

        public async Task<PartialViewResult> Versions(int id, int page = 1)
        {
            var webpage = await _webpageAdminService.GetWebpage(id);
            ViewData["page"] = page;
            return PartialView(webpage);
        }

        public async Task<JsonResult> Select2Search(string term, int page = 1)
        {
            var data = await _webpageAdminService.Search(term, page);
            return Json(new
            {
                total = data.TotalItemCount, items = data.ToList()
            });
        }
    }
}