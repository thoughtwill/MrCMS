using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MrCMS.Web.Apps.Core.Services;

public interface IGetTagPageSelectListItemService
{
    Task<IEnumerable<SelectListItem>> Get(int? id);
}