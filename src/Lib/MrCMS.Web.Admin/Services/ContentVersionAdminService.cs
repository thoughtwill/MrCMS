using MrCMS.Entities.Documents.Web;
using MrCMS.Helpers;
using MrCMS.Services;
using MrCMS.Web.Admin.Models.Content;
using NHibernate;
using NHibernate.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MrCMS.Web.Admin.Services;

public class ContentVersionAdminService : IContentVersionAdminService
{
    private readonly ISession _session;
    private readonly IGetHomePage _getHomePage;

    public ContentVersionAdminService(ISession session, IGetHomePage getHomePage)
    {
        _session = session;
        _getHomePage = getHomePage;
    }

    public async Task<IReadOnlyList<ContentVersion>> GetVersions(int webpageId)
    {
        return await _session.Query<ContentVersion>()
            .Where(x => x.Webpage.Id == webpageId)
            .OrderByDescending(x => x.CreatedOn)
            .ToListAsync();
    }

    public async Task<ContentVersion> AddInitialContentVersion(AddInitialContentVersionModel model)
    {
        var webpage = _session.Get<Webpage>(model.WebpageId);
        if (webpage == null)
            return null;

        var contentVersion = new ContentVersion { Webpage = webpage, Status = ContentVersionStatus.Draft };
        webpage.ContentVersions.Add(contentVersion);
        await _session.TransactAsync(session => session.SaveAsync(contentVersion));

        return contentVersion;
    }

    public async Task<ContentVersionModel> GetEditModel(int id)
    {
        var version = await _session.GetAsync<ContentVersion>(id);
        
        if (version == null)
            return null;

        var previewUrl = $"/{version.Webpage.UrlSegment}?version={version.Id}";
        if ((await _getHomePage.Get()).Id == version.Webpage.Id)
        {
            previewUrl = $"/?version={version.Id}";
        }

        return new ContentVersionModel
        {
            Id = version.Id,
            WebpageId = version.Webpage.Id,
            PreviewUrl = previewUrl,
            Blocks = version.Blocks.OrderBy(f => f.Order).Select(x =>
            {
                var blockModel = x.DeserializeData();
                var metadata = ContentBlockMappings.BlockMetadata[x.Type];
                var contentVersionBlockSummaryModel = new ContentVersionBlockSummaryModel
                {
                    Id = x.Id,
                    Order = x.Order,
                    IsHidden = x.IsHidden,
                    Type = metadata.Type,
                    DisplayName = string.IsNullOrWhiteSpace(blockModel.DisplayName) ? metadata.Name : blockModel.DisplayName,
                    CanAddChildren = metadata.CanAddChildren,
                    CanOrderChildren = metadata.CanOrderChildren,
                    Guid = x.Guid,
                    Items = blockModel.Items.Select(y => new ContentVersionBlockItemSummaryModel
                    {
                        Id = y.Id,
                        Name = y.GetDisplayName(blockModel),
                        Type = y.GetType().FullName
                    }).ToList()
                };
                return contentVersionBlockSummaryModel;
            }).ToList()
        };
    }

    public async Task<ContentVersionActionResult> Publish(int id)
    {
        var version = await _session.GetAsync<ContentVersion>(id);
        if (version == null)
            return new ContentVersionActionResult { Success = false, Message = "Could not find draft" };
        if (!version.IsDraft)
            return new ContentVersionActionResult
            {
                Success = false, Message = "This content is not a draft and therefore cannot be published",
                WebpageId = version.Webpage.Id
            };
        // make current live archived
        var existingLive = version.Webpage.ContentVersions.Where(x => x.Status == ContentVersionStatus.Live).ToList();
        foreach (var contentVersion in existingLive) contentVersion.Status = ContentVersionStatus.Archived;

        version.Status = ContentVersionStatus.Live;

        await _session.TransactAsync(async session =>
        {
            foreach (var contentVersion in existingLive)
            {
                await session.UpdateAsync(contentVersion);
            }

            await session.UpdateAsync(version);
        });

        return new ContentVersionActionResult
            { Success = true, Message = "Draft made live", WebpageId = version.Webpage.Id };
    }

    public async Task<ContentVersionActionResult> Delete(int id)
    {
        var version = await _session.GetAsync<ContentVersion>(id);
        if (version == null)
            return new ContentVersionActionResult { Success = false, Message = "Could not find draft" };
        if (!version.IsDraft)
            return new ContentVersionActionResult
            {
                Success = false, Message = "This content is not a draft and therefore cannot be deleted",
                WebpageId = version.Webpage.Id
            };

        await _session.TransactAsync(session => session.DeleteAsync(version));

        return new ContentVersionActionResult
            { Success = true, Message = "Draft deleted", WebpageId = version.Webpage.Id };
    }

    public async Task<ContentVersion> AddDraftBasedOnVersion(ContentVersion liveVersion)
    {
        var newVersion = liveVersion.CreateDraft();
        newVersion.Webpage.ContentVersions.Add(newVersion);
        await _session.TransactAsync(session => session.SaveAsync(newVersion));

        return newVersion;
    }
}