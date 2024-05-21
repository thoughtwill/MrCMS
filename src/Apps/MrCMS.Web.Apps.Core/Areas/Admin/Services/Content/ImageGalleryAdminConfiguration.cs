using MrCMS.Web.Admin.Infrastructure.Services.Content;
using MrCMS.Web.Apps.Core.Areas.Admin.Models.Content;
using MrCMS.Web.Apps.Core.Entities.ContentBlocks;

namespace MrCMS.Web.Apps.Core.Areas.Admin.Services.Content;

public class
    ImageGalleryAdminConfiguration : ContentBlockAdminConfigurationBase<ImageGallery, UpdateImageGalleryAdminModel>
{
    public override UpdateImageGalleryAdminModel GetEditModel(ImageGallery block)
    {
        return new UpdateImageGalleryAdminModel
        {
            ItemPerRow = GetItemPerRow(block.ResponsiveClasses),
            ImageRatio = block.ImageRatio,
            ImageRenderSize = block.ImageRenderSize,
            BackgroundColour = block.BackgroundColour
        };
    }

    public override void UpdateBlock(ImageGallery block, UpdateImageGalleryAdminModel editModel)
    {
        switch (editModel.ItemPerRow)
        {
            case 6:
                block.ResponsiveClasses = "col-sm-6 col-md-4 col-lg-2";
                break;
            case 4:
                block.ResponsiveClasses = "col-sm-6 col-md-3";
                break;
            case 3:
                block.ResponsiveClasses = "col-sm-4";
                break;
            case 2:
                block.ResponsiveClasses = "col-sm-6";
                break;
            case 1:
                block.ResponsiveClasses = "col-12";
                break;
            default:
                block.ResponsiveClasses = "col-sm-6 col-md-3";
                break;
        }

        block.ImageRatio = editModel.ImageRatio;
        block.ImageRenderSize = (7 - editModel.ItemPerRow) * 200;
        block.BackgroundColour = editModel.BackgroundColour;
    }

    private int GetItemPerRow(string responsiveClasses)
    {
        return responsiveClasses switch
        {
            "col-sm-6 col-md-4 col-lg-2" => 6,
            "col-sm-6 col-md-3" => 4,
            "col-sm-4" => 3,
            "col-sm-6" => 2,
            "col-12" => 1,
            _ => 4
        };
    }
}