using System.Collections.Generic;
using MrCMS.Messages;

namespace MrCMS.Web.Admin.Services
{
    public interface IMessageTemplateTokensAdminService
    {
        HashSet<string> GetTokens(MessageTemplate messageTemplate);
    }
}