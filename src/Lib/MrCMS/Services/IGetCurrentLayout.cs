using MrCMS.Entities.Documents.Layout;
using MrCMS.Entities.Documents.Web;

namespace MrCMS.Services
{
    public interface IGetCurrentLayout
    {
        Layout GetSiteDefault();

        Layout Get(Webpage webpage);
        Layout GetUserAccountLayout();
    }
}