using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MrCMS.AI.Entities;
using MrCMS.AI.Models;
using NHibernate;
using NHibernate.Linq;

namespace MrCMS.Web.Admin.Infrastructure.Services;

public class PromptService : IPromptService
{
    private readonly ISession _session;

    public PromptService(ISession session)
    {
        _session = session;
    }
    
    public async Task<List<Prompt>> GetPrompts(PromptType type)
    {
        return await _session.Query<Prompt>().Where(x => x.Type == type).ToListAsync();
    }
}