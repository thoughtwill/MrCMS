using System.Threading.Tasks;
using MrCMS.ContentTemplates.Entities;

namespace MrCMS.ContentTemplates.Services;

public interface IGetContentTemplateService
{
    Task<ContentTemplate> Get(int id);
}