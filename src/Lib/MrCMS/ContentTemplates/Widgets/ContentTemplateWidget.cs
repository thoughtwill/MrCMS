using MrCMS.ContentTemplates.Entities;
using MrCMS.Entities.Widget;
using MrCMS.Website;

namespace MrCMS.ContentTemplates.Widgets;

[WidgetOutputCacheable]
public class ContentTemplateWidget : Widget
{
    public virtual ContentTemplate ContentTemplate { get; set; }

    public virtual string Properties { get; set; }
}