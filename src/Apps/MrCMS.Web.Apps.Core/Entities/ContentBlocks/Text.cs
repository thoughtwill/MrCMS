using System.Collections.Generic;
using MrCMS.Entities.Documents.Web;
using MrCMS.Web.Apps.Core.Areas.Admin.Models;

namespace MrCMS.Web.Apps.Core.Entities.ContentBlocks;

public class Text : IContentBlock
{
    public string DisplayName => "Text";
    public string Heading { get; set; }
    public string Subtext { get; set; }
    public BackgroundColour BackgroundColour { get; set; } = BackgroundColour.Grey;
    public IReadOnlyList<BlockItem> Items => new BlockItem[] { };
}