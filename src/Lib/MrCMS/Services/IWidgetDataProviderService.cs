using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MrCMS.Services;

public interface IWidgetDataProviderService
{
    Task<List<SelectListItem>> WidgetTypeDropdownItems();
    Type GetTypeByName(string typeName);
}