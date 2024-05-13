using System;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MrCMS.Entities.Messaging;
using MrCMS.Settings;

namespace MrCMS.Services
{
    public class EmailSender : IEmailSender, IDisposable
    {
        private readonly ILogger<EmailSender> _logger;
        private readonly SmtpClient _smtpClient;

        public EmailSender(MailSettings mailSettings, ILogger<EmailSender> logger, IGetSmtpClient getSmtpClient)
        {
            _logger = logger;
            _smtpClient = getSmtpClient.GetClient(mailSettings);
        }

        public void Dispose()
        {
            _smtpClient.Dispose();
        }

        public bool CanSend(QueuedMessage queuedMessage)
        {
            return !string.IsNullOrEmpty(queuedMessage.ToAddress) && _smtpClient.Credentials != null &&
                   !string.IsNullOrWhiteSpace(_smtpClient.Host);
        }

        public async Task<QueuedMessage> SendMailMessage(QueuedMessage queuedMessage)
        {
            try
            {
                var mailMessage = BuildMailMessage(queuedMessage);

                await _smtpClient.SendMailAsync(mailMessage);
                queuedMessage.SentOn = DateTime.UtcNow;
            }
            catch (Exception exception)
            {
                _logger.LogInformation(exception, "Email Sending Failed {ExceptionMessage} {ExceptionStackTrace}", exception.Message, exception.StackTrace);
                queuedMessage.Tries++;
            }

            return queuedMessage;
        }


        private MailMessage BuildMailMessage(QueuedMessage queuedMessage)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(queuedMessage.FromAddress ?? "", queuedMessage.FromName ?? ""),
                Subject = queuedMessage.Subject,
                Body = queuedMessage.Body
            };
            var multipleToAddress = queuedMessage.ToAddress?.Split(new[] {',', ';'},
                StringSplitOptions.RemoveEmptyEntries);
            if (multipleToAddress != null && multipleToAddress.Any())
            {
                foreach (var email in multipleToAddress)
                {
                    mailMessage.To.Add(new MailAddress(email.Trim(), queuedMessage.ToName));
                }
            }

            multipleToAddress = queuedMessage.Cc?.Split(new[] { ',', ';' },
                StringSplitOptions.RemoveEmptyEntries);
            if (multipleToAddress?.Any() ?? false)
            {
                foreach (var email in multipleToAddress)
                {
                    mailMessage.CC.Add(new MailAddress(email.Trim(), queuedMessage.ToName));
                }
            }

            multipleToAddress = queuedMessage.Bcc?.Split(new[] { ',', ';' },
                StringSplitOptions.RemoveEmptyEntries);
            if (multipleToAddress?.Any() ?? false)
            {
                foreach (var email in multipleToAddress)
                {
                    mailMessage.Bcc.Add(new MailAddress(email.Trim(), queuedMessage.ToName));
                }
            }

            multipleToAddress = queuedMessage.ReplayTo?.Split(new[] { ',', ';' },
                StringSplitOptions.RemoveEmptyEntries);
            if (multipleToAddress?.Any() ?? false)
            {
                foreach (var email in multipleToAddress)
                {
                    mailMessage.ReplyToList.Add(new MailAddress(email.Trim(), queuedMessage.FromName));
                }
            }

            if (queuedMessage.QueuedMessageAttachments != null)
                foreach (var attachment in queuedMessage.QueuedMessageAttachments)
                    mailMessage.Attachments.Add(new Attachment(new MemoryStream(attachment.Data), attachment.FileName,
                        attachment.ContentType));

            mailMessage.IsBodyHtml = queuedMessage.IsHtml;
            return mailMessage;
        }
    }
}