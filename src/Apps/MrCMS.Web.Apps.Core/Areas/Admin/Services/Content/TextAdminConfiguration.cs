using MrCMS.Web.Admin.Infrastructure.Services.Content;
using MrCMS.Web.Apps.Core.Areas.Admin.Models.Content;
using MrCMS.Web.Apps.Core.Entities.ContentBlocks;

namespace MrCMS.Web.Apps.Core.Areas.Admin.Services.Content;

public class TextAdminConfiguration : ContentBlockAdminConfigurationBase<Text, UpdateTextAdminModel>
{
    public override UpdateTextAdminModel GetEditModel(Text block)
    {
        return new UpdateTextAdminModel
            { Heading = block.Heading, Subtext = block.Subtext, BackgroundColour = block.BackgroundColour };
    }

    public override void UpdateBlock(Text block, UpdateTextAdminModel editModel)
    {
        block.Heading = editModel.Heading;
        block.Subtext = editModel.Subtext;
        block.BackgroundColour = editModel.BackgroundColour;
    }
}