using System.Collections.Generic;

namespace MrCMS.ContentTemplates.Models;

public struct ParseNode
{
    public ParseNode(string text)
    {
        Text = text;
        TypeName = null;
        Name = null;
        Attributes = null;
        Children = null;
    }

    public ParseNode(string name, string typeName, AttributeItem[] attributes)
    {
        Text = null;
        TypeName = typeName;
        Name = name;
        Attributes = attributes;
        Children = null;
    }

    public string Text { get; }
    public string TypeName { get; }
    public string Name { get; }
    public AttributeItem[] Attributes { get; }

    public List<ParseNode> Children { get; set; }
}