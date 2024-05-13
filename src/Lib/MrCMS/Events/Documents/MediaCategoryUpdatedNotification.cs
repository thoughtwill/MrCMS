using System.Threading.Tasks;
using MrCMS.Entities.Documents.Media;
using MrCMS.Entities.Notifications;
using MrCMS.Services.Notifications;

namespace MrCMS.Events.Documents
{
    public class MediaCategoryUpdatedNotification : IOnUpdated<MediaCategory>
    {
        private readonly IGetNotificationModifiedUserInfo _getNotificationModifiedUserInfo;
        private readonly INotificationPublisher _notificationPublisher;

        public MediaCategoryUpdatedNotification(IGetNotificationModifiedUserInfo getNotificationModifiedUserInfo, INotificationPublisher notificationPublisher)
        {
            _getNotificationModifiedUserInfo = getNotificationModifiedUserInfo;
            _notificationPublisher = notificationPublisher;
        }

        public async Task Execute(OnUpdatedArgs<MediaCategory> args)
        {
            var webpage = args.Item;
            var message = string.Format("<a href=\"/Admin/MediaCategory/Edit/{1}\">{0}</a> has been updated{2}.",
                webpage.Name,
                webpage.Id, await _getNotificationModifiedUserInfo.GetInfo());
            await _notificationPublisher.PublishNotification(message, PublishType.Both, NotificationType.AdminOnly);
        }
    }
}