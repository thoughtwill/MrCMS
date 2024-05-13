using System;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MrCMS.Models;
using MrCMS.Services.Resources;
using MrCMS.Settings;

namespace MrCMS.Services
{
    public class TestSmtpSettings : ITestSmtpSettings
    {
        private readonly IGetSmtpClient _getSmtpClient;

        private readonly ILogger<TestSmtpSettings> _logger;

        //private readonly ErrorSignal _errorSignal;
        private readonly IStringResourceProvider _resourceProvider;
        private readonly ICurrentSiteLocator _siteLocator;

        public TestSmtpSettings(IGetSmtpClient getSmtpClient, ILogger<TestSmtpSettings> logger,
            IStringResourceProvider resourceProvider, ICurrentSiteLocator siteLocator)
        {
            _getSmtpClient = getSmtpClient;
            _logger = logger;
            _resourceProvider = resourceProvider;
            _siteLocator = siteLocator;
        }

        public async Task<bool> TestSettings(MailSettings settings, TestEmailInfo info)
        {
            try
            {
                using (var smtpClient = _getSmtpClient.GetClient(settings))
                {
                    var site = _siteLocator.GetCurrentSite();
                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(settings.SystemEmailAddress),
                        Subject = "SMTP Test",
                        Body = await _resourceProvider.GetValue("Admin - Test Email - Content", options =>
                            options.SetDefaultValue(
                                "Testing email functionality from " + site.DisplayName)),
                    };
                    mailMessage.To.Add(new MailAddress(info.Email));

                    smtpClient.Send(mailMessage);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex, ex.Message);
                return false;
            }
        }
    }
}