using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;
using MrCMS.AI.Entities;
using MrCMS.DbConfiguration;

namespace MrCMS.AI.DbConfiguration.Overrides;

public class PromptOverride : IAutoMappingOverride<Prompt>
{
    public void Override(AutoMapping<Prompt> mapping)
    {
        mapping.Map(prompt => prompt.Name).Length(100);
        mapping.Map(prompt => prompt.Template).MakeVarCharMax();
        
        mapping.Table("Prompts");
    }
}