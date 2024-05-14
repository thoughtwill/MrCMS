using System.Collections.Generic;
using System.Threading.Tasks;
using MrCMS.Entities.Documents.Layout;
using MrCMS.Entities.Widget;

namespace MrCMS.Website
{
    public interface IWidgetLoader
    {
        Task<IList<Widget>> GetWidgets(LayoutArea area);
        Task<IReadOnlyDictionary<LayoutArea, IList<Widget>>> GetWidgets(IEnumerable<LayoutArea> layoutAreas);
    }
}