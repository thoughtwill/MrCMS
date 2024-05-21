using System.Threading.Tasks;
using MrCMS.Installation.Models;
using MrCMS.Services;
using MrCMS.Settings;

namespace MrCMS.Installation.Services
{
    public class InitializeDatabase : IInitializeDatabase
    {
        private readonly IConfigurationProvider _configurationProvider;
        private readonly ISystemConfigurationProvider _systemConfigurationProvider;

        public InitializeDatabase(IConfigurationProvider configurationProvider,
            ISystemConfigurationProvider systemConfigurationProvider)
        {
            _configurationProvider = configurationProvider;
            _systemConfigurationProvider = systemConfigurationProvider;
        }

        public async Task Initialize(InstallModel model)
        {
            SetupTasks();
            var siteSettings = new SiteSettings
            {
                UICulture = model.UiCulture,
                EnableInlineEditing = true,
                FormRendererType = FormRenderingType.Bootstrap5,
                FormLabelRenderingType = FormLabelRenderingType.Normal
            };
            var mediaSettings = new MediaSettings
            {
                ThumbnailImageHeight = 50,
                ThumbnailImageWidth = 50,
                LargeImageHeight = 800,
                LargeImageWidth = 800,
                MediumImageHeight = 500,
                MediumImageWidth = 500,
                SmallImageHeight = 200,
                SmallImageWidth = 200,
                ResizeQuality = 90,
            };

            var fileSystemSettings = new FileSystemSettings
            {
                StorageType = typeof(FileSystem).FullName
            };
            var mailSettings = new MailSettings { SystemEmailAddress = model.AdminEmail };

            await _configurationProvider.SaveSettings(siteSettings);
            await _configurationProvider.SaveSettings(mediaSettings);
            await _configurationProvider.SaveSettings(fileSystemSettings);
            await _systemConfigurationProvider.SaveSettings(mailSettings);
        }

        private void SetupTasks()
        {
            // _taskSettingManager.Update(typeof(DeleteExpiredLogsTask), true, TODO);
            //_taskSettingManager.Update(typeof(DeleteOldQueuedTasks), true, 600);
            // _taskSettingManager.Update(typeof(SendQueuedMessagesTask), false, TODO);
            // _taskSettingManager.Update(typeof(PublishScheduledWebpagesTask), true, TODO);
            // _taskSettingManager.Update(typeof(UpdateSitemap), true, TODO);
        }
    }
}