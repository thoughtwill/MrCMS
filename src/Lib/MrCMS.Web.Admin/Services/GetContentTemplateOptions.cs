using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using MrCMS.ContentTemplates.Entities;
using MrCMS.Helpers;
using NHibernate;

namespace MrCMS.Web.Admin.Services;

public class GetContentTemplateOptions : IGetContentTemplateOptions
{
    private readonly ISession _session;

    public GetContentTemplateOptions(ISession session)
    {
        _session = session;
    }

    public async Task<IList<SelectListItem>> GetOptions(Func<ContentTemplate, Task<bool>> selected = null,
        string emptyItemText = null, string group = null)
    {
        var templates = await _session.QueryOver<ContentTemplate>()
            .OrderBy(template => template.Name).Asc
            .Cacheable().ListAsync();
        return await templates
            .BuildSelectItemListAsync(template => Task.FromResult(template.Name),
                template => Task.FromResult(template.Id.ToString()),
                selected,
                emptyItemText, group);
    }
}