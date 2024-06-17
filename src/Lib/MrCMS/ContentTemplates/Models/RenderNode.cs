using System.Collections.Generic;

namespace MrCMS.ContentTemplates.Models;

public struct RenderNode
{
    public RenderNode(string name, string typeName, AttributeItem[] attributes)
    {
        Name = name;
        TypeName = typeName;
        Attributes = attributes;
        Children = null;
    }

    public string TypeName { get; }
    public string Name { get; }
    public AttributeItem[] Attributes { get; }

    public List<RenderNode> Children { get; set; }
}