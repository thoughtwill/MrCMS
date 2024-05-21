using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using MrCMS.Entities.Documents.Web;
using MrCMS.Helpers;
using MrCMS.Services;
using MrCMS.Services.Resources;
using MrCMS.Web.Admin.Models.Search;
using NHibernate;
using NHibernate.Criterion;
using X.PagedList;
using QuickSearchResult = MrCMS.Web.Admin.Models.QuickSearchResult;

namespace MrCMS.Web.Admin.Services
{
    public class AdminWebpageSearchService : IAdminWebpageSearchService
    {
        private readonly IGetBreadcrumbs _getBreadcrumbs;
        private readonly IWebpageMetadataService _webpageMetadataService;
        private readonly ISession _session;
        private readonly ICurrentSiteLocator _siteLocator;
        private readonly IStringResourceProvider _stringResourceProvider;

        public AdminWebpageSearchService(
            IGetBreadcrumbs getBreadcrumbs, ISession session, ICurrentSiteLocator siteLocator,
            IStringResourceProvider stringResourceProvider,
            IWebpageMetadataService webpageMetadataService)
        {
            _getBreadcrumbs = getBreadcrumbs;
            _session = session;
            _siteLocator = siteLocator;
            _stringResourceProvider = stringResourceProvider;
            _webpageMetadataService = webpageMetadataService;
        }

        public IPagedList<Webpage> Search(AdminWebpageSearchQuery model)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<QuickSearchResult> QuickSearch(AdminWebpageSearchQuery model)
        {
            throw new NotImplementedException();

            // return Enumerable.Select(_documentSearcher.Search(GetQuery(model), model.Page, 10), x =>
            //     new QuickSearchResult
            //     {
            //         id = x.Id,
            //         value = x.Name,
            //         url = _getLiveUrl.GetAbsoluteUrl(x)
            //     });
        }


        public async Task<IReadOnlyList<Webpage>> GetBreadCrumb(int? parentId)
        {
            return await _getBreadcrumbs.Get(parentId);
        }

        public async Task<List<SelectListItem>> GetWebpageTypes(string type)
        {
            return _webpageMetadataService.GetWebpageMetadata()
                .BuildSelectItemList(definition => definition.Name, definition => definition.TypeName,
                    definition => definition.TypeName == type,
                    await _stringResourceProvider.GetValue("Admin Select Type", configureOptions => configureOptions.SetDefaultValue("Select type")));
        }

        public async Task<IList<SelectListItem>> GetParentsList()
        {
            var parentIds = (await _session.QueryOver<Webpage>()
                .Where(webpage => webpage.Parent != null)
                .SelectList(
                    builder =>
                        builder.Select(
                            Projections.Distinct(Projections.Property<Webpage>(webpage => webpage.Parent.Id))))
                .Cacheable()
                .ListAsync<int>()).ToList();
            var site = _siteLocator.GetCurrentSite();
            var rootWebpages = await _session.QueryOver<Webpage>()
                .Where(webpage => webpage.Parent == null && webpage.Site.Id == site.Id && webpage.Id.IsIn(parentIds))
                .OrderBy(webpage => webpage.DisplayOrder)
                .Asc.ListAsync();
            var selectListItems = await GetPageListItems(
                rootWebpages, parentIds, 1);
            selectListItems.Insert(0,
                new SelectListItem
                {
                    Selected = false,
                    Text = await _stringResourceProvider.GetValue("Admin Root",
                        options => options.SetDefaultValue("Root")),
                    Value = "0"
                });
            return selectListItems;
        }

        private async Task<IList<SelectListItem>> GetPageListItems(IEnumerable<Webpage> pages,
            List<int> parentIds, int depth)
        {
            var items = new List<SelectListItem>();

            foreach (var node in pages.Where(node => parentIds.Contains(node.Id)))
            {
                items.Add(new SelectListItem
                {
                    Selected = false,
                    Text = GetDashes(depth) + node.Name,
                    Value = node.Id.ToString()
                });
                items.AddRange(await GetPageListItems(await _session.QueryOver<Webpage>()
                    .Where(webpage => webpage.Parent.Id == node.Id && webpage.Id.IsIn(parentIds))
                    .OrderBy(webpage => webpage.DisplayOrder)
                    .Asc.Cacheable()
                    .ListAsync(), parentIds, depth + 1));
            }

            return items;
        }

        private string GetDashes(int depth)
        {
            return string.Empty.PadRight(depth * 2, '-');
        }
    }
}