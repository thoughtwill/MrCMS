using System.Collections.Generic;
using System.Threading.Tasks;
using MrCMS.Entities.Documents;
using MrCMS.Entities.Documents.Web;

namespace MrCMS.Services
{
    public interface IDocumentTagsUpdateService
    {
        Task SetTags(string taglist, int id);
        Task SetTags(string taglist, Document document);
        Task SetTags(List<string> taglist, Document document);
    }
}