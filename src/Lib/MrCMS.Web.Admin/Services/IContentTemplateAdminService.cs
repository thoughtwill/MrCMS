using System.Threading.Tasks;
using MrCMS.ContentTemplates.Entities;
using MrCMS.Web.Admin.Models;
using X.PagedList;

namespace MrCMS.Web.Admin.Services;

public interface IContentTemplateAdminService
{
    Task<IPagedList<ContentTemplate>> SearchAsync(ContentTemplateSearchModel searchModel);
    Task AddAsync(AddContentTemplateModel addContentTemplateModel);
    Task<ContentTemplate> GetAsync(int id);
    Task<bool> IsUniqueName(string name, int? id);
    Task<UpdateContentTemplateModel> GetUpdateModel(int id);
    Task UpdateAsync(UpdateContentTemplateModel model);
    Task DeleteAsync(int id);
}