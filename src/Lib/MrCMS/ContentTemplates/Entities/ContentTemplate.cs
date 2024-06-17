using MrCMS.Entities;

namespace MrCMS.ContentTemplates.Entities;

public class ContentTemplate : SiteEntity
{
    public virtual string Name { get; set; }
    public virtual string Text { get; set; }
}