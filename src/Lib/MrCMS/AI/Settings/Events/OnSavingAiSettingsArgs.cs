using MrCMS.Settings;

namespace MrCMS.AI.Settings.Events
{
    public class OnSavingAiSettingsArgs<T> where T : AiSettingsBase
    {
        private readonly T _settings;
        private readonly T _original;

        public OnSavingAiSettingsArgs(T settings, T original)
        {
            _settings = settings;
            _original = original;
        }

        public T Settings => _settings;

        public T Original => _original;
    }
}