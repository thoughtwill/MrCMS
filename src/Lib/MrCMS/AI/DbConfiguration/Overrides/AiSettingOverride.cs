using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;
using MrCMS.AI.Entities.Settings;
using MrCMS.DbConfiguration.Types;

namespace MrCMS.AI.DbConfiguration.Overrides
{
    public class AiSettingOverride : IAutoMappingOverride<AiSetting>
    {
        public void Override(AutoMapping<AiSetting> mapping)
        {
            mapping.Map(setting => setting.Value).CustomType<VarcharMax>().Length(4001);

            mapping.Map(setting => setting.SettingType).Length(120);
            mapping.Map(setting => setting.PropertyName).Length(50);
        }
    }
}