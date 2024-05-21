using System.Collections.Generic;
using MrCMS.Entities.Documents.Web;
using MrCMS.Web.Apps.Core.Areas.Admin.Models;

namespace MrCMS.Web.Apps.Core.Entities.ContentBlocks;

public class Quote : IContentBlock
{
    public string DisplayName => "Quote";
    public string QuoteText { get; set; }
    public string QuoteFooter { get; set; }
    public string CssClasses { get; set; } = "border-left border-secondary pl-2";
    public BackgroundColour BackgroundColour { get; set; } = BackgroundColour.White;
    public IReadOnlyList<BlockItem> Items => new BlockItem[] { };
}