using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MrCMS.Web.Apps.Core.Services;

public class GetTagPageSelectListItemService : IGetTagPageSelectListItemService
{
    private readonly ITagPageUIService _pageUiService;

    public GetTagPageSelectListItemService(ITagPageUIService pageUiService)
    {
        _pageUiService = pageUiService;
    }
    
    public async Task<IEnumerable<SelectListItem>> Get(int? id)
    {
        var primaryTagOptions = new List<SelectListItem>();
        if (!id.HasValue) return primaryTagOptions;
        
        var primaryTag = await _pageUiService.GetPage(id.Value);
        primaryTagOptions.Add(new SelectListItem
        {
            Text = primaryTag.Name,
            Value = primaryTag.Id.ToString(),
            Selected = true
        });

        return primaryTagOptions;
    }
}