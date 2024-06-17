using System;
using System.Text.RegularExpressions;
using MrCMS.ContentTemplates.Models;

namespace MrCMS.ContentTemplates.Helper;

public static class AttributesHelper
{
    private static readonly Regex AttributesMatcher =
        new(@"\s(\w+)=\""([^""]*)\""|'([^']*)'", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static AttributeItem[] GetAttributes(string text)
    {
        var matchCollection = AttributesMatcher.Matches(text);
        var attributes = new AttributeItem[matchCollection.Count];
        for (var i = 0; i < matchCollection.Count; i++)
        {
            var match = matchCollection[i].Value.AsSpan();
            var index = match.IndexOf("=\"");

            var key = match[..index].Trim();
            var value = match.Slice(index + 2, match.Length - index - 2).TrimEnd("\"").Trim();

            attributes[i] = new AttributeItem(key.ToString(), value.ToString());
        }

        return attributes;
    }
}