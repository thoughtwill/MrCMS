using System.Linq;
using System.Threading.Tasks;
using MrCMS.AI;
using MrCMS.AI.Entities;
using MrCMS.AI.Models;
using MrCMS.Helpers;
using MrCMS.Web.Admin.Models.Prompts;
using NHibernate;
using NHibernate.Linq;
using X.PagedList;

namespace MrCMS.Web.Admin.Services;

public class PromptAdminService : IPromptAdminService
{
    private readonly ISession _session;

    public PromptAdminService(ISession session)
    {
        _session = session;
    }

    public async Task<IPagedList<Prompt>> GetPrompts(PromptSearchModel search)
    {
        var query = _session.Query<Prompt>();

        if (!string.IsNullOrWhiteSpace(search.Name)) query = query.Where(x => x.Name.Contains(search.Name));

        query = query.Where(x => x.Type == search.Type);

        return await query.OrderBy(x => x.Name).PagedAsync(search.Page);
    }


    public async Task<UpdatePromptModel> AddPrompt(AddPromptModel model)
    {
        var prompt = new Prompt
        {
            Name = model.Name,
            Template = model.Template,
            Type = model.Type
        };

        await _session.TransactAsync(session => session.SaveAsync(prompt));

        return await GetEditModel(prompt.Id);
    }

    public async Task<UpdatePromptModel> GetEditModel(int id)
    {
        var prompt = await GetPrompt(id);

        if (prompt == null)
            return null;

        return new UpdatePromptModel
        {
            Id = prompt.Id,
            Name = prompt.Name,
            Template = prompt.Template,
            Type = prompt.Type
        };
    }

    public async Task<bool> IsUniqueName(string name, PromptType type, int? id)
    {
        return !await _session.Query<Prompt>()
            .AnyAsync(f => f.Name.ToLower() == name.ToLower() && f.Type == type && id != f.Id);
    }

    private async Task<Prompt> GetPrompt(int id)
    {
        return await _session.GetAsync<Prompt>(id);
    }

    public async Task<UpdatePromptModel> UpdatePrompt(UpdatePromptModel model)
    {
        var prompt = await GetPrompt(model.Id);

        if (prompt == null)
            return null;

        prompt.Name = model.Name;
        prompt.Template = model.Template;

        await _session.TransactAsync(session => session.UpdateAsync(prompt));

        return await GetEditModel(prompt.Id);
    }

    public async Task DeletePrompt(int id)
    {
        var prompt = await GetPrompt(id);

        if (prompt == null)
            return;

        await _session.TransactAsync(session => session.DeleteAsync(prompt));
    }
}