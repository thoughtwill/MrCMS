using MrCMS.Web.Admin.Infrastructure.ModelBinding;
using MrCMS.Web.Apps.Core.Widgets;

namespace MrCMS.Web.Apps.Core.Areas.Admin.Models.Widgets
{
    public class CodeWidgetModel : IUpdatePropertiesViewModel<CodeWidget>, IAddPropertiesViewModel<CodeWidget>
    {
        public string Text { get; set; }
    }
}