using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MrCMS.ContentTemplates.ContentTemplateTokenProviders.Base;
using MrCMS.ContentTemplates.Helper;
using MrCMS.ContentTemplates.Models;
using StackExchange.Profiling;

namespace MrCMS.ContentTemplates.Core;

public class ContentTemplateViewRenderer : IContentTemplateViewRenderer
{
    private static readonly Regex ShortcodeMatcher =
        new(@"\[([\w-_]+)([^\]]*)?\]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex ShortcodeSplitMatcher =
        new(@"(\[[\w-_]+[^\]]*?\])", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private const string CheckboxPrefix = "CBI_";


    private readonly Dictionary<string, ContentTemplateTokenProvider> _parsers;

    public ContentTemplateViewRenderer(IEnumerable<ContentTemplateTokenProvider> parsers)
    {
        _parsers = parsers.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
    }

    public bool CanParse(string typeName)
    {
        if (ContainerTokens.Get().Any(f =>
                string.Equals(typeName, f.HtmlToken, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(typeName, $"{f.HtmlToken}-end", StringComparison.OrdinalIgnoreCase)))
            return false;

        return _parsers.ContainsKey(typeName);
    }

    public async Task<IHtmlContent> ParseAsync(IHtmlHelper helper, string text, string props)
    {
        using (MiniProfiler.Current.Step("ContentTemplateParse"))
        {
            var matches = string.IsNullOrWhiteSpace(text) ? Array.Empty<string>() : ShortcodeSplitMatcher.Split(text);
            var tree = BuildPropertiesTree(matches, false);

            if (string.IsNullOrEmpty(props))
                return new HtmlString(ClearShortCodes(helper, tree));

            var properties = JToken.Parse(props);

            var result = await ParseShortcodes(helper, tree, properties, properties);
            return result;
        }
    }


    #region Tree

    private List<ParseNode> BuildPropertiesTree(string[] matchCollection, bool isArray)
    {
        var index = 0;
        return GetChildren(matchCollection, ref index, true);
    }


    public List<ParseNode> GetChildren(string[] matchCollection, ref int i, bool topLevel)
    {
        var tree = new List<ParseNode>();
        for (; i < matchCollection.Length; i++)
        {
            var text = matchCollection[i];

            //skip html parts
            if (text.StartsWith('[') && text.EndsWith(']'))
            {
                var match = ShortcodeMatcher.Match(text);
                if (match.Groups.Count != 3)
                    continue;

                var typeName = match.Groups[1].Value;
                if (!topLevel && ContainerTokens.Get().Any(f =>
                        string.Equals(typeName, $"{f.HtmlToken}-end", StringComparison.OrdinalIgnoreCase)))
                    return tree;

                var attributes = AttributesHelper.GetAttributes(match.Groups[2].Value);
                if (attributes.Length == 0)
                    continue;

                var nameValueTuple = attributes.FirstOrDefault(x => x.Key == "name");
                if (string.IsNullOrEmpty(nameValueTuple.Value))
                    continue;

                var node = new ParseNode(nameValueTuple.Value, typeName, attributes);
                if (ContainerTokens.Get()
                    .Any(f => string.Equals(typeName, f.HtmlToken, StringComparison.OrdinalIgnoreCase)))
                {
                    i++;
                    node.Children = GetChildren(matchCollection, ref i, false);
                }

                tree.Add(node);
            }
            else
            {
                tree.Add(new ParseNode(text));
            }
        }

        if (!topLevel && matchCollection.Length == i)
            throw new Exception($"array dose not have a closing tag");

        return tree;
    }

    #endregion

    #region Parsing

    private string ClearShortCodes(IHtmlHelper helper, List<ParseNode> tree)
    {
        var stb = new StringBuilder();
        foreach (var node in tree.Where(node => node.Text != null))
        {
            stb.Append(node.Text);
        }

        return stb.ToString();
    }

    private async Task<IHtmlContent> ParseShortcodes(IHtmlHelper helper, List<ParseNode> tree, JToken properties,
        JToken globalProperties = null,
        int? index = null)
    {
        var htmlContent = new HtmlContentBuilder();
        foreach (var node in tree)
        {
            if (node.Text != null)
                htmlContent.AppendHtml(new HtmlString(node.Text));
            else if (node.Children != null && node.Children.Any())
                htmlContent.AppendHtml(await ParseArray(helper, node, properties, globalProperties));
            else if (node is { Name: not null, TypeName: not null })
                htmlContent.AppendHtml(await ParseProperty(helper, node, node.Attributes, properties, globalProperties,
                    index));
        }

        return htmlContent;
    }

    private async Task<IHtmlContent> ParseArray(IHtmlHelper helper, ParseNode node, JToken props = null,
        JToken globalProps = null)
    {
        if (props == null)
            return HtmlString.Empty;

        var name = node.Name;

        var prop = props[name];
        if (prop is not JArray array)
            return HtmlString.Empty;

        var htmlContent = new HtmlContentBuilder();
        foreach (var item in array.Select((value, index) => (value, index)))
        {
            if (item.value.Value<string>(CheckboxPrefix) != "false")
                htmlContent.AppendHtml(
                    await ParseShortcodes(helper, node.Children, item.value, globalProps, item.index));
        }

        return htmlContent;
    }

    private async Task<IHtmlContent> ParseProperty(IHtmlHelper helper, ParseNode node, AttributeItem[] attributes,
        JToken props = null, JToken globalProps = null, int? index = null)
    {
        var typeName = node.TypeName;
        var parser = _parsers[typeName];

        var properties = parser.GlobalRender ? globalProps : props;

        if (properties == null)
            return HtmlString.Empty;

        if (!CanParse(typeName))
            return HtmlString.Empty;


        var name = node.Name;
        var property = new ViewRenderElementProperty
        {
            Name = name,
            Value = null,
            Index = index,
            Attributes = attributes
        };
        
        if (properties[name] == null)
        {
            return await _parsers[typeName].ViewRenderAsync(helper, property);
        }

        if (properties.Value<string>($"{CheckboxPrefix}{name}") == "false")
            return HtmlString.Empty;


        property.Value = properties.Value<string>(name);
        return await _parsers[typeName]
            .ViewRenderAsync(helper, property);
    }

    #endregion
}