using System.Collections.Generic;
using System.Threading.Tasks;
using MrCMS.Entities.Documents.Media;
using MrCMS.Models;
using MrCMS.Web.Admin.Models;

namespace MrCMS.Web.Admin.Services
{
    public interface IMediaCategoryAdminService
    {
        AddMediaCategoryModel GetNewCategoryModel(int? id);
        Task<MediaCategory> GetCategory(int? id);
        Task<MediaCategory> Add(AddMediaCategoryModel model);
        Task<MediaCategory> Update(UpdateMediaCategoryModel model);
        Task<MediaCategory> Delete(int id);
        Task<List<SortItem>> GetSortItems(int id);
        Task SetOrders(List<SortItem> items);
        Task<bool> UrlIsValidForMediaCategory(int siteId, string urlSegment, int? id);
        Task<UpdateMediaCategoryModel> GetEditModel(int id);
        Task<MediaCategory> Get(int id);
        Task<CanAddCategoryResult> CanAdd(AddMediaCategoryModel model);

        public class CanAddCategoryResult
        {
            public bool Success { get; set; }
            public string ErrorMessage { get; set; }
        }
    }
}