using System.ComponentModel;

namespace MrCMS.Web.Apps.Core.Areas.Admin.Models.Content;

public class UpdateImageGalleryAdminModel
{
    [DisplayName("Item per row")] public int ItemPerRow { get; set; } = 4;
    public string ImageRatio { get; set; }
    public int ImageRenderSize { get; set; }
    public BackgroundColour BackgroundColour { get; set; }
}