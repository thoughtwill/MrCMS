using System.Collections.Generic;
using MrCMS.Entities.Widget;
using MrCMS.Web.Apps.Core.Models.Widgets;
using MrCMS.Website;

namespace MrCMS.Web.Apps.Core.Widgets;

[WidgetOutputCacheable(PerPage = true)]
public class SliderWidget : Widget
{
    public virtual string SlideList { get; set; }
    
    public virtual int Interval { get; set; }

    public virtual bool ShowIndicator { get; set; }

    public virtual bool PauseOnHover { get; set; }
    public virtual string CaptionCssClass { get; set; } = "d-none d-md-block";
    public virtual string DesktopSlideRatio { get; set; } = "19x6";
    public virtual string MobileSlideRatio { get; set; } = "1x1";
    
    public virtual string BackgroundColor { get; set; } = "transparent";

    public virtual IList<SlideViewModel> Slides => Newtonsoft.Json.JsonConvert.DeserializeObject<List<SlideViewModel>>(SlideList ?? "");
}