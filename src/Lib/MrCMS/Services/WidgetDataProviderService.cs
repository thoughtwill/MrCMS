using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using MrCMS.ContentTemplates.Entities;
using MrCMS.Entities.Widget;
using MrCMS.Helpers;
using NHibernate;
using NHibernate.Linq;

namespace MrCMS.Services;

public class WidgetDataProviderService
{
    private readonly ISession _session;

    private static IEnumerable<Type> _widgetTypes;

    private static IEnumerable<Type> WidgetTypes =>
        _widgetTypes ??= TypeHelper.GetAllConcreteMappedClassesAssignableFrom<Widget>();

    public WidgetDataProviderService(ISession session)
    {
        _session = session;
    }

    public async Task<List<SelectListItem>> WidgetTypeDropdownItems()
    {
            
            var widgetSelectListItems = WidgetTypes.OrderBy(x => x.Name).BuildSelectItemList(
                type => type.Name.BreakUpString(),
                type => type.FullName,
                emptyItemText: null, group: "Widgets");
            
            //Load content templates
            var contentTemplates = await _session.Query<ContentTemplate>().OrderBy(f => f.Name).ToListAsync();
            if (contentTemplates?.Any() ?? false)
            {
                contentTemplates.BuildSelectItemList(f=>f.Name,f=>f.Id.ToString(), emptyItemText: null, group: "Content Templates")
                    .ForEach(x => widgetSelectListItems.Add(x));
            }

            return widgetSelectListItems;
    }

    public Type GetTypeByName(string typeName)
    {
        return WidgetTypes.FirstOrDefault(x => x.FullName == typeName);
    }
}