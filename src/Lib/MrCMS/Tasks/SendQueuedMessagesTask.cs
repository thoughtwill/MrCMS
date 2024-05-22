using System;
using System.Threading.Tasks;
using Hangfire;
using MrCMS.DbConfiguration;
using MrCMS.Entities.Messaging;
using MrCMS.Helpers;
using MrCMS.Services;
using NHibernate;

namespace MrCMS.Tasks
{
    public class SendQueuedMessagesTask : ISendQueuedMessagesTask
    {
        public const int MAX_TRIES = 5;
        protected readonly ISession _session;
        private readonly IEmailSender _emailSender;

        public SendQueuedMessagesTask(ISession session, IEmailSender emailSender)
        {
            _session = session;
            _emailSender = emailSender;
        }

        [DisableConcurrentExecution(timeoutInSeconds: 3600)]
        public async Task Execute()
        {
            using (new SiteFilterDisabler(_session))
            {
                await _session.TransactAsync(async session =>
                {
                    foreach (
                        QueuedMessage queuedMessage in
                        await session.QueryOver<QueuedMessage>().Where(
                                message => message.SentOn == null && message.Tries < MAX_TRIES)
                            .ListAsync())
                    {
                        var message = queuedMessage;
                        if (_emailSender.CanSend(message))
                            message = await _emailSender.SendMailMessage(message);
                        else
                            message.SentOn = DateTime.UtcNow;
                        await session.SaveOrUpdateAsync(message);
                    }
                });
            }
        }
    }

    public interface ISendQueuedMessagesTask
    {
        Task Execute();
    }
}