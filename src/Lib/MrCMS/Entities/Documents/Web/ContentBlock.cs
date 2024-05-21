using System;
using MrCMS.Helpers;
using Newtonsoft.Json;

namespace MrCMS.Entities.Documents.Web;

public class ContentBlock : SiteEntity
{
    private static readonly JsonSerializerSettings SerializerSettings = new()
    {
        ContractResolver = new WritablePropertiesOnlyResolver()
    };

    public virtual int Order { get; set; }
    public virtual bool IsHidden { get; set; }
    public virtual string Type { get; set; }
    public virtual string Data { get; set; }
    public virtual ContentVersion ContentVersion { get; set; }

    public virtual void SerializeData(object model)
    {
        var type = model.GetType();
        Type = type.FullName;
        Data = JsonConvert.SerializeObject(model, type, Formatting.None, SerializerSettings);
    }

    public virtual IContentBlock DeserializeData()
    {
        var type = TypeHelper.GetTypeByName(Type);
        if (type == null)
            return null;
        return JsonConvert.DeserializeObject(Data, type, SerializerSettings) as IContentBlock;
    }

    public virtual ContentBlock Clone(ContentVersion contentVersion)
    {
        return new ContentBlock
        {
            Data = Data,
            Order = Order,
            Type = Type,
            IsHidden = IsHidden,
            Site = Site,
            ContentVersion = contentVersion
        };
    }
}

// public class ContentBlockMetadataAttribute : Attribute
// {
//     public ContentBlockMetadataAttribute(string displayName, string editorType)
//     {
//         DisplayName = displayName;
//         EditorType = editorType;
//     }
//
//     public string DisplayName { get; }
//     public string EditorType { get; }
// }
// public class ContentBlockItemMetadataAttribute : Attribute
// {
//     public ContentBlockItemMetadataAttribute(string displayName, string editorType)
//     {
//         DisplayName = displayName;
//         EditorType = editorType;
//     }
//
//     public string DisplayName { get; }
//     public string EditorType { get; }
// }
