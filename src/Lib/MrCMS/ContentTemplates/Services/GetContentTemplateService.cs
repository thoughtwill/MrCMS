using System.Threading.Tasks;
using MrCMS.ContentTemplates.Entities;
using NHibernate;

namespace MrCMS.ContentTemplates.Services;

public class GetContentTemplateService : IGetContentTemplateService
{
    private readonly ISession _session;

    public GetContentTemplateService(ISession session)
    {
        _session = session;
    }

    public async Task<ContentTemplate> Get(int id)
    {
        return await _session.GetAsync<ContentTemplate>(id);
    }
}