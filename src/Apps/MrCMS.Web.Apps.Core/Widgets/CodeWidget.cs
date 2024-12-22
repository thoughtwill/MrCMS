using MrCMS.Entities.Widget;
using MrCMS.Website;

namespace MrCMS.Web.Apps.Core.Widgets
{
    [WidgetOutputCacheable]
    public class CodeWidget : Widget
    {
        public virtual string Text { get; set; }
    }
}