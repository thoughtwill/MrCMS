using MrCMS.ContentTemplates.Entities;
using MrCMS.ContentTemplates.Widgets;
using MrCMS.Web.Admin.Infrastructure.ModelBinding;

namespace MrCMS.Web.Admin.Models.Widgets;

public class ContentTemplateWidgetUpdateModel : IUpdatePropertiesViewModel<ContentTemplateWidget>
{
    public ContentTemplate ContentTemplate { get; set; }
    public string Properties { get; set; }
}