using System.IO;
using System.Threading.Tasks;
using MrCMS.Entities.Documents.Web;
using MrCMS.Web.Admin.Models;
using MrCMS.Web.Admin.Models.Redirects;
using X.PagedList;

namespace MrCMS.Web.Admin.Services
{
    public interface IRedirectsAdminService
    {
        Task<IPagedList<UrlHistory>> SearchAll(RedirectsSearchQuery searchQuery);
        Task<IPagedList<UrlHistory>> SearchKnown404s(Known404sSearchQuery searchQuery);
        Task<UrlHistory> GetUrlHistory(int id);
        Task MarkAsGone(int id);
        Task MarkAsIgnored(int id);
        Task Reset(int id);
        Task Remove(int id);
        Task<SetRedirectUrlModel> GetSetRedirectUrlModel(int id);
        Task SetRedirectUrl(SetRedirectUrlModel model);
        Task<SetRedirectPageModel> GetSetRedirectPageModel(int id);
        Task SetRedirectPage(SetRedirectPageModel model);
        Task<RedirectImportResult> ImportRedirects(Stream fileStream);
    }
}