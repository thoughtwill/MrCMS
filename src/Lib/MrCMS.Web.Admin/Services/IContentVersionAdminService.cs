using System.Collections.Generic;
using System.Threading.Tasks;
using MrCMS.Entities.Documents.Web;
using MrCMS.Web.Admin.Models.Content;

namespace MrCMS.Web.Admin.Services;

public interface IContentVersionAdminService
{
    Task<IReadOnlyList<ContentVersion>> GetVersions(int webpageId);
    Task<ContentVersion> AddInitialContentVersion(AddInitialContentVersionModel model);
    Task<ContentVersionModel> GetEditModel(int id);
    Task<ContentVersionActionResult> Publish(int id);
    Task<ContentVersionActionResult> Delete(int id);
    Task<ContentVersion> AddDraftBasedOnVersion(ContentVersion liveVersion);
    Task CopyContentVersion(int sourceId, int targetId);
}