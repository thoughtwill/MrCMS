using System.Collections.Generic;
using MrCMS.Messages;
using MrCMS.Services;

namespace MrCMS.Web.Admin.Services
{
    public class MessageTemplateTokensAdminService : IMessageTemplateTokensAdminService
    {
        private readonly IMessageTemplateParser _messageTemplateParser;

        public MessageTemplateTokensAdminService(IMessageTemplateParser messageTemplateParser)
        {
            _messageTemplateParser = messageTemplateParser;
        }

        public HashSet<string> GetTokens(MessageTemplate messageTemplate)
        {
            return _messageTemplateParser.GetAllTokens(messageTemplate);
        }
    }
}