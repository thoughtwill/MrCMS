using System.Collections.Generic;
using MrCMS.Entities.Documents.Web;

namespace MrCMS.ContentTemplates.Entities.ContentBlocks;

public class ContentTemplateBlock: IContentBlock
{
    public string DisplayName => string.IsNullOrWhiteSpace(Name) ? "Content Template" : Name;
    
    public string Name { get; set; }
    
    public IReadOnlyList<BlockItem> Items => new BlockItem[] { };
    
    public int? ContentTemplateId { get; set; }
    public string Properties { get; set; }
    
}