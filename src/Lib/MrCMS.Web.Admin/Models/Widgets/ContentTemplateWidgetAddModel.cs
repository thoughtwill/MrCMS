using System.ComponentModel;
using MrCMS.ContentTemplates.Widgets;
using MrCMS.Web.Admin.Infrastructure.ModelBinding;

namespace MrCMS.Web.Admin.Models.Widgets;

public class ContentTemplateWidgetAddModel : IAddPropertiesViewModel<ContentTemplateWidget>
{
    [DisplayName("Content Template")]
    public int ContentTemplateId { get; set; }
}