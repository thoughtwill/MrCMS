using System.Collections.Generic;
using System.Threading.Tasks;
using MrCMS.Entities.Documents.Web;
using MrCMS.Web.Apps.Core.Models.Search;
using X.PagedList;

namespace MrCMS.Web.Apps.Core.Services.Search
{
    public interface IWebpageSearchService
    {
        Task<IPagedList<Webpage>> Search(WebpageSearchQuery model);
        Task<IReadOnlyList<Webpage>> GetBreadCrumb(int? parentId);
    }
}
