using System.Collections.Generic;
using MrCMS.ContentTemplates.Models;

namespace MrCMS.ContentTemplates.Helper;

public static class ContainerTokens
{
    private static List<ContainerTokenItem> _containerTokens = new List<ContainerTokenItem>
    {
        new ContainerTokenItem
        {
            HtmlToken = "array",
            Type = ContainerTokenType.Repeatable
        },
        new ContainerTokenItem
        {
            HtmlToken = "html",
            Type = ContainerTokenType.Container
        }
    };
    
    public static List<ContainerTokenItem> Get()
    {
        return _containerTokens;
    }
}