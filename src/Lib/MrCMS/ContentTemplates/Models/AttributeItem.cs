namespace MrCMS.ContentTemplates.Models;

public struct AttributeItem
{
    public AttributeItem(string key, string value)
    {
        Key = key;
        Value = value;
    }

    public string Key { get; }
    public string Value { get; }
}