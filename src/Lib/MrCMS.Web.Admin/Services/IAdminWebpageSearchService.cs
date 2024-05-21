using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using MrCMS.Entities.Documents.Web;
using MrCMS.Web.Admin.Models.Search;
using X.PagedList;
using QuickSearchResult = MrCMS.Web.Admin.Models.QuickSearchResult;

namespace MrCMS.Web.Admin.Services
{
    public interface IAdminWebpageSearchService
    {
        IPagedList<Webpage> Search(AdminWebpageSearchQuery model);
        IEnumerable<QuickSearchResult> QuickSearch(AdminWebpageSearchQuery model);
        Task<IReadOnlyList<Webpage>> GetBreadCrumb(int? parentId);
        Task<List<SelectListItem>> GetWebpageTypes(string type);
        Task<IList<SelectListItem>> GetParentsList();
    }
}