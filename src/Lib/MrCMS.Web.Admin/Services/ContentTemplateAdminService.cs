using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MrCMS.ContentTemplates.Entities;
using MrCMS.Helpers;
using MrCMS.Web.Admin.Models;
using NHibernate;
using NHibernate.Linq;
using X.PagedList;

namespace MrCMS.Web.Admin.Services;

public class ContentTemplateAdminService : IContentTemplateAdminService
{
    private readonly IMapper _mapper;
    private readonly ISession _session;

    public ContentTemplateAdminService(ISession session, IMapper mapper)
    {
        _session = session;
        _mapper = mapper;
    }

    public async Task<IPagedList<ContentTemplate>> SearchAsync(ContentTemplateSearchModel searchModel)
    {
        var query = _session.Query<ContentTemplate>();

        if (!string.IsNullOrWhiteSpace(searchModel.Name)) query = query.Where(x => x.Name.Contains(searchModel.Name));

        return await query.OrderBy(x => x.Name).PagedAsync(searchModel.Page);
    }

    public async Task AddAsync(AddContentTemplateModel addContentTemplateModel)
    {
        var contentTemplate = _mapper.Map<ContentTemplate>(addContentTemplateModel);

        await _session.TransactAsync(s => s.SaveAsync(contentTemplate));
    }

    public async Task<ContentTemplate> GetAsync(int id)
    {
        return await _session.GetAsync<ContentTemplate>(id);
    }

    public async Task<bool> IsUniqueName(string name, int? id)
    {
        return !await _session.Query<ContentTemplate>().AnyAsync(f => f.Name.ToLower() == name.ToLower() && id != f.Id);
    }

    public async Task<UpdateContentTemplateModel> GetUpdateModel(int id)
    {
        var template = await GetAsync(id);
        return _mapper.Map<UpdateContentTemplateModel>(template);
    }

    public async Task UpdateAsync(UpdateContentTemplateModel model)
    {
        var contentTemplate = await GetAsync(model.Id);
        _mapper.Map(model, contentTemplate);
        await _session.TransactAsync(session => session.UpdateAsync(contentTemplate));
    }

    public async Task DeleteAsync(int id)
    {
        var contentTemplate = await GetAsync(id);

        await _session.TransactAsync(s => s.DeleteAsync(contentTemplate));
    }
}