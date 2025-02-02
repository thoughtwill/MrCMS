using MrCMS.Events;
using MrCMS.Settings;

namespace MrCMS.AI.Settings.Events
{
    public interface IOnSavingAiSettings<T> : IEvent<OnSavingAiSettingsArgs<T>> where T : AiSettingsBase
    {

    }
}