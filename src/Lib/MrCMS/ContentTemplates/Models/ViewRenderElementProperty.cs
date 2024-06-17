namespace MrCMS.ContentTemplates.Models;

public class ViewRenderElementProperty
{
    public string Name { get; set; }
    public string Value { get; set; }
    public int? Index { get; set; }
    public AttributeItem[] Attributes { get; set; }
}