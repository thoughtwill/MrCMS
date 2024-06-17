using System.Text.Json;

namespace MrCMS.ContentTemplates.Helper;

public static class JsonExtensions
{
    public static JsonElement? GetNullableProperty(this JsonElement jsonElement, string propertyName)
    {
        if (jsonElement.ValueKind == JsonValueKind.Null || jsonElement.ValueKind == JsonValueKind.Undefined)
            return null;

        if (jsonElement.TryGetProperty(propertyName, out var returnElement))
            return returnElement;

        return null;
    }
}