using System;
using System.Threading.Tasks;
using MrCMS.Entities.Notifications;
using MrCMS.Entities.People;
using MrCMS.Helpers;
using NHibernate;

namespace MrCMS.Services.Notifications
{
    public class NotificationPublisher : INotificationPublisher
    {
        private readonly ISession _session;
        private readonly IGetCurrentUser _getCurrentUser;
        private readonly IEventContext _eventContext;

        public NotificationPublisher(ISession session, IGetCurrentUser getCurrentUser, IEventContext eventContext)
        {
            _session = session;
            _getCurrentUser = getCurrentUser;
            _eventContext = eventContext;
        }

        public async Task PublishNotification(string message, PublishType publishType = PublishType.Both,
            NotificationType notificationType = NotificationType.All)
        {
            if (_session.GetContext().AreNotificationsDisabled())
                return;

            var user = await _getCurrentUser.Get();
            // load the user from the current session
            if (user != null)
                user = _session.Get<User>(user.Id);
            
            var notification = new Notification
            {
                Message = message,
                User = user,
                NotificationType = notificationType
            };
            switch (publishType)
            {
                case PublishType.Transient:
                    await PushNotification(notification);
                    break;
                case PublishType.Persistent:
                    await SaveNotification(notification);
                    break;
                case PublishType.Both:
                    await SaveNotification(notification);
                    await PushNotification(notification);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(publishType));
            }
        }

        private async Task SaveNotification(Notification notification)
        {
            await _session.TransactAsync(async (session, token) => await session.SaveAsync(notification, token));
            await _eventContext.Publish<IOnPersistentNotificationPublished, OnPersistentNotificationPublishedEventArgs>(
                new OnPersistentNotificationPublishedEventArgs(notification));
        }

        private async Task PushNotification(Notification notification)
        {
            await _eventContext.Publish<IOnTransientNotificationPublished, OnTransientNotificationPublishedEventArgs>(
                new OnTransientNotificationPublishedEventArgs(notification));
        }
    }
}