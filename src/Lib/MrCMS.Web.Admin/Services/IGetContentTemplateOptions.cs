using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using MrCMS.ContentTemplates.Entities;

namespace MrCMS.Web.Admin.Services;

public interface IGetContentTemplateOptions
{
    Task<IList<SelectListItem>> GetOptions(Func<ContentTemplate, Task<bool>> selected = null,
        string emptyItemText = null, string group = null);
}