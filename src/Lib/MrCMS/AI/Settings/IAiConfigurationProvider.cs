using System.Collections.Generic;
using System.Threading.Tasks;

namespace MrCMS.AI.Settings
{
    public interface IAiConfigurationProvider 
    {
        TSettings GetSettings<TSettings>() where TSettings : AiSettingsBase, new();
        Task SaveSettings(AiSettingsBase settings);
        Task SaveSettings<T>(T settings) where T : AiSettingsBase, new();
        Task DeleteSettings<T>(T settings)where T : AiSettingsBase, new();
        List<AiSettingsBase> GetAllSettings();
    }
}