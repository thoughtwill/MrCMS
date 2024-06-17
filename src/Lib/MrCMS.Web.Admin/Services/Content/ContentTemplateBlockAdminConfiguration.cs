using MrCMS.ContentTemplates.Entities.ContentBlocks;
using MrCMS.Web.Admin.Infrastructure.Services.Content;
using MrCMS.Web.Admin.Models.Content;

namespace MrCMS.Web.Admin.Services.Content;

public class ContentTemplateBlockAdminConfiguration : ContentBlockAdminConfigurationBase<ContentTemplateBlock,
    UpdateContentTemplateBlockModel>
{
    public override UpdateContentTemplateBlockModel GetEditModel(ContentTemplateBlock block)
    {
        return new UpdateContentTemplateBlockModel
            { ContentTemplateId = block.ContentTemplateId, Name = block.Name, Properties = block.Properties };
    }

    public override void UpdateBlock(ContentTemplateBlock block, UpdateContentTemplateBlockModel editModel)
    {
        block.ContentTemplateId = editModel.ContentTemplateId;
        block.Name = editModel.Name;
        block.Properties = editModel.Properties;
    }
}