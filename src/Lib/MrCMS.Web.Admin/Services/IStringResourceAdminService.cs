using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using MrCMS.Entities.Resources;
using MrCMS.Web.Admin.Models;
using X.PagedList;

namespace MrCMS.Web.Admin.Services
{
    public interface IStringResourceAdminService
    {
        Task<IPagedList<StringResource>> Search(StringResourceSearchQuery searchQuery);
        Task Add(AddStringResourceModel resource);
        Task<StringResource> GetResource(int id);
        Task<StringResource> GetResource(string key);
        UpdateStringResourceModel GetEditModel(StringResource resource);
        Task Update(UpdateStringResourceModel model);
        Task<SaveResult> Update(StringResourceInlineUpdateModel model);
        Task Delete(int id);
        Task<List<SelectListItem>> GetLanguageOptions(string key, int? siteId);
        Task<List<SelectListItem>> SearchLanguageOptions();
        Task<AddStringResourceModel> GetNewResource(string key, int? id);
        Task<List<SelectListItem>> ChooseSiteOptions(ChooseSiteParams chooseSiteParams);
        Task<List<SelectListItem>> SearchSiteOptions();
    }
}