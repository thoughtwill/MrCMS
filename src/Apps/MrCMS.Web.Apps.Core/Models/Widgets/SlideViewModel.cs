using System.ComponentModel.DataAnnotations;

namespace MrCMS.Web.Apps.Core.Models.Widgets;

public class SlideViewModel
{
    [Required]
    public string Image { get; set; }
    public string SmallImage { get; set; }
    public string Caption { get; set; }
    public string Link { get; set; }
}