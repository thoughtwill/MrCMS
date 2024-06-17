using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;
using MrCMS.ContentTemplates.Widgets;

namespace MrCMS.ContentTemplates.DbConfiguration;

public class ContentTemplateWidgetMappingOverride: IAutoMappingOverride<ContentTemplateWidget>
{
    public void Override(AutoMapping<ContentTemplateWidget> mapping)
    {
        mapping.References(f => f.ContentTemplate, "ContentTemplateId");
    }
}