namespace MrCMS.ContentTemplates.Models;

public class AdminRenderElementProperty
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Label { get; set; }
    public string Value { get; set; }
    public AttributeItem[] Attributes { get; set; }
}