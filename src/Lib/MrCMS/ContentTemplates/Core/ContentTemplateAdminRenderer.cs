using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using MrCMS.ContentTemplates.ContentTemplateTokenProviders.Base;
using MrCMS.Helpers;
using MrCMS.Shortcodes.Forms;
using MrCMS.ContentTemplates.Helper;
using MrCMS.ContentTemplates.Models;

namespace MrCMS.ContentTemplates.Core;

public class ContentTemplateAdminRenderer : IContentTemplateAdminRenderer
{
    private static readonly Regex ShortcodeMatcher =
        new(@"\[([\w-_]+)([^\]]*)?\]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly Dictionary<string, ContentTemplateTokenProvider> _renderers;

    private readonly string _checkboxPrefix = "CBI_";

    public ContentTemplateAdminRenderer(IEnumerable<ContentTemplateTokenProvider> renderers)
    {
        _renderers = renderers.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
    }

    private bool CanRender(string typeName)
    {
        return _renderers.ContainsKey(typeName);
    }

    private bool IsContainer(string typeName)
    {
        return ContainerTokens.Get().Any(f =>
            string.Equals(typeName, f.HtmlToken, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(typeName, $"{f.HtmlToken}-end", StringComparison.OrdinalIgnoreCase));
    }

    public async Task<IHtmlContent> RenderAsync(IHtmlHelper helper, string text, JsonElement properties)
    {
        if (string.IsNullOrEmpty(text))
            return HtmlString.Empty;

        return await RenderProperties(helper, text, properties);
    }

    private async Task<IHtmlContentBuilder> RenderProperties(IHtmlHelper helper, string text, JsonElement properties)
    {
        var htmlContent = new HtmlContentBuilder();
        var matchCollection = ShortcodeMatcher.Matches(text);
        if (!matchCollection.Any())
            return htmlContent;

        var treeNodeList = BuildPropertiesTree(matchCollection);

        var container = new TagBuilder("div");
        container.AddCssClass("content-template-container");

        var row = new TagBuilder("div");
        row.AddCssClass("row");

        //TODO
        //check if it can run in parallel
        foreach (var node in treeNodeList)
            row.InnerHtml.AppendHtml(await RenderProperties(helper, node, properties));

        container.InnerHtml.AppendHtml(row);

        htmlContent.AppendHtml(container);

        return htmlContent;
    }

    private async Task<IHtmlContent> RenderProperties(IHtmlHelper helper, RenderNode node, JsonElement properties,
        string prefix = null, bool setValue = true)
    {
        return node.Children != null && node.Children.Any()
            ? await RenderArray(helper, node, properties, prefix, setValue)
            : await RenderProperty(helper, node, properties, prefix, setValue);
    }

    #region Tree

    private List<RenderNode> BuildPropertiesTree(MatchCollection matchCollection)
    {
        var tree = new List<RenderNode>();
        for (var i = 0; i < matchCollection.Count; i++)
        {
            var match = matchCollection[i];
            if (match.Groups.Count != 3)
                continue;

            var attributes = AttributesHelper.GetAttributes(match.Groups[2].Value);
            if (attributes.Length == 0)
                continue;

            var nameValueTuple = attributes.FirstOrDefault(x => x.Key == "name");
            if (string.IsNullOrEmpty(nameValueTuple.Value))
                continue;

            var typeName = match.Groups[1].Value;
            var node = new RenderNode(nameValueTuple.Value, typeName, attributes);
            if (ContainerTokens.Get()
                .Any(f => string.Equals(typeName, f.HtmlToken, StringComparison.OrdinalIgnoreCase)))
                node.Children = GetChildren(matchCollection, ref i, typeName);

            if (tree.All(x => x.Name != nameValueTuple.Value))
                tree.Add(node);
        }

        return tree;
    }

    private List<RenderNode> GetChildren(MatchCollection matchCollection, ref int i, string parentName)
    {
        var children = new List<RenderNode>();
        for (i += 1; i < matchCollection.Count; i++)
        {
            var match = matchCollection[i];
            if (match.Groups.Count != 3)
                continue;

            var typeName = match.Groups[1].Value;
            if (ContainerTokens.Get()
                .Any(f => string.Equals(typeName, $"{f.HtmlToken}-end", StringComparison.OrdinalIgnoreCase)))
                return children;

            var attributes = AttributesHelper.GetAttributes(match.Groups[2].Value);
            if (attributes.Length == 0)
                continue;

            var nameValueTuple = attributes.FirstOrDefault(x => x.Key == "name");
            if (string.IsNullOrEmpty(nameValueTuple.Value))
                continue;

            var node = new RenderNode(nameValueTuple.Value, typeName, attributes);
            if (ContainerTokens.Get()
                .Any(f => string.Equals(typeName, f.HtmlToken, StringComparison.OrdinalIgnoreCase)))
                node.Children = GetChildren(matchCollection, ref i, nameValueTuple.Value);

            if (children.All(x => x.Name != nameValueTuple.Value))
                children.Add(node);
        }

        throw new Exception($"array '{parentName}' dose not have a closing tag");
    }

    #endregion

    #region List Render

    private async Task<IHtmlContent> RenderArray(IHtmlHelper helper, RenderNode node, JsonElement properties,
        string prefix = null, bool setValue = true)
    {
        var name = node.Name;
        var containerTokenType = ContainerTokens.Get().FirstOrDefault(f =>
                                         string.Equals(node.TypeName, f.HtmlToken, StringComparison.OrdinalIgnoreCase))
                                     ?.Type ??
                                 ContainerTokenType.Repeatable;
        var props = new JsonElement();
        if (properties.ValueKind != JsonValueKind.Null && properties.ValueKind != JsonValueKind.Undefined)
            properties.TryGetProperty(name, out props);


        //Card responsive can change from here
        var responsive = new TagBuilder("div");
        responsive.AddCssClass("col-12");

        //Table
        var container = new TagBuilder("div");
        container.AddCssClass($"content-template-{node.TypeName}-container");

        //Add headers
        var header = GetContainerHeader(name.BreakUpString());
        container.InnerHtml.AppendHtml(header);

        prefix = $"{prefix}[{name}]";

        if (containerTokenType == ContainerTokenType.Repeatable)
        {
            //Add row Template
            var rowTemplate = new TagBuilder("template");
            rowTemplate.Attributes["data-row-template"] = null;

            rowTemplate.InnerHtml.AppendHtml(await GetDataTemplate(node.Children, props, helper, containerTokenType,
                prefix));

            container.InnerHtml.AppendHtml(rowTemplate);
        }

        //Add rows
        var rows = await GetDataCards(node.Children, props, helper, containerTokenType, prefix, setValue);
        container.InnerHtml.AppendHtml(rows);

        if (containerTokenType == ContainerTokenType.Repeatable)
        {
            var addButton = GetContainerAddButton(name.BreakUpString());
            container.InnerHtml.AppendHtml(addButton);
        }

        responsive.InnerHtml.AppendHtml(container);

        return responsive;
    }

    #endregion

    #region Property Render

    private async Task<IHtmlContent> RenderProperty(IHtmlHelper helper, RenderNode node, JsonElement properties,
        string prefix, bool setValue = true)
    {
        var typeName = node.TypeName;
        var label = node.Name;

        if (!CanRender(typeName) || IsContainer(typeName))
            return null;

        var existingProperty = properties.GetNullableProperty(label);
        var existingValue = existingProperty?.GetString();

        var isArray = prefix != null;

        prefix = (prefix == null || _renderers[typeName].GlobalRender) ? label : $"{prefix}[{label}]";

        var elementContainer = GetPropertyContainer();
        var elementHtml = await GetProperty(helper, typeName, label, prefix, node.Attributes,
            setValue ? existingValue : null);

        if (!isArray) //instead of using show label
        {
            var enabledValue = existingProperty.HasValue
                ? properties.GetProperty($"{_checkboxPrefix}{prefix}").GetString()
                : "true";
            var enabledHtml = GetEnabledProperty(prefix, enabledValue);
            elementContainer.InnerHtml.AppendHtml(enabledHtml);
        }


        elementContainer.InnerHtml.AppendHtml(elementHtml);

        var responsive = new TagBuilder("div");
        responsive.AddCssClass(_renderers[typeName].AdminRenderResponsiveClass);

        responsive.InnerHtml.AppendHtml(elementContainer);

        return responsive;
    }

    private async Task<IHtmlContent> GetProperty(IHtmlHelper helper, string typeName, string label, string name,
        AttributeItem[] attributes, string existingValue)
    {
        IHtmlContentBuilder elementHtml = new HtmlContentBuilder();

        var property = new AdminRenderElementProperty
        {
            Id = $"el-{Guid.NewGuid():N}",
            Name = name,
            Label = label,
            Value = existingValue,
            Attributes = attributes
        };
        
        elementHtml.AppendHtml(GetLabel(property));

        var render = await _renderers[typeName].AdminRenderAsync(helper, property);
        elementHtml.AppendHtml(render);

        return elementHtml;
    }

    private IHtmlContent GetLabel(AdminRenderElementProperty property)
    {
        var tagBuilder = new TagBuilder("label");
        property.Label = property.Label.Split('(')[0];

        tagBuilder.Attributes["for"] = property.Id;

        tagBuilder.InnerHtml.Append(property.Label.BreakUpString());

        return tagBuilder;
    }

    private TagBuilder GetPropertyContainer()
    {
        var elementContainer = new TagBuilder("div");
        elementContainer.AddCssClass("form-group");
        return elementContainer;
    }

    private IHtmlContent GetEnabledProperty(string name, string existingValue, bool addMargin = true,
        int? index= null)
    {
        var tagBuilder = new TagBuilder("input");
        if (addMargin)
            tagBuilder.AddCssClass("mr-2");
        tagBuilder.Attributes["type"] = "checkbox";
        tagBuilder.Attributes["name"] = index.HasValue ? $"{name}[][{_checkboxPrefix}]" : $"{_checkboxPrefix}{name}";
        tagBuilder.Attributes["value"] = "true";
        if (existingValue == "true")
            tagBuilder.Attributes["checked"] = "checked";

        tagBuilder.Attributes["data-content-template-input"] = null;

        return tagBuilder;
    }

    #endregion

    #region TableHeader

    private IHtmlContent GetContainerHeader(string title)
    {
        var header = new TagBuilder("h4");

        header.AddCssClass("border-bottom pb-3 my-3");

        header.InnerHtml.AppendHtml(title);

        return header;
    }

    private IHtmlContent GetContainerAddButton(string name)
    {
        var addButton = new TagBuilder("div");
        addButton.AddCssClass("row my-3 justify-content-center");

        addButton.InnerHtml.AppendHtml(
            $"<div class='col-auto'><button data-array-id='{Guid.NewGuid()}' type='button' class='btn btn-primary add-row'><i class='fa fa-plus'></i> Add item to {name}</button></div>");

        return addButton;
    }

    #endregion

    #region TableRows

    private async Task<IHtmlContentBuilder> GetDataTemplate(List<RenderNode> nodes, JsonElement props,
        IHtmlHelper helper, ContainerTokenType type,
        string prefix = null)
    {
        IHtmlContentBuilder bodyHtml = new HtmlContentBuilder();

        var card = new TagBuilder("div");
        var nodeProps = props.ValueKind == JsonValueKind.Undefined ? new JsonElement() : props[0];
        var enabledValue = nodeProps.GetNullableProperty(_checkboxPrefix)?.GetString() ?? "true";

        card.AddCssClass("card mb-3");

        card.InnerHtml.AppendHtml(GetCardHeader(prefix, enabledValue, 0, type));

        var cardBody = new TagBuilder("div");
        cardBody.AddCssClass("card-body py-2");

        var row = new TagBuilder("div");
        row.AddCssClass("row");

        var prefixName = $"{prefix}[]";

        foreach (var node in nodes)
        {
            var render = await RenderProperties(helper, node, nodeProps, prefixName, false);
            if (render != null)
                row.InnerHtml.AppendHtml(render);
        }

        cardBody.InnerHtml.AppendHtml(row);
        card.InnerHtml.AppendHtml(cardBody);

        bodyHtml.AppendHtml(card);

        return bodyHtml;
    }

    private async Task<IHtmlContentBuilder> GetDataCards(List<RenderNode> nodes, JsonElement props,
        IHtmlHelper helper, ContainerTokenType type,
        string prefix = null, bool setValue = true)
    {
        IHtmlContentBuilder bodyHtml = new HtmlContentBuilder();

        var i = 0;
        var existingPropsCount =
            props.ValueKind == JsonValueKind.Undefined ? 0 :
            props.ValueKind == JsonValueKind.Array ? props.EnumerateArray().Count() :
            props.EnumerateObject().Count();

        //always add at least a row
        do
        {
            var card = new TagBuilder("div");
            var nodeProps = props.ValueKind == JsonValueKind.Undefined ? new JsonElement() : props[i];
            var enabledValue = nodeProps.GetNullableProperty(_checkboxPrefix)?.GetString() ?? "true";

            card.AddCssClass("card mb-3");

            card.InnerHtml.AppendHtml(GetCardHeader(prefix, enabledValue, i, type));

            var cardBody = new TagBuilder("div");
            cardBody.AddCssClass("card-body py-2");

            var row = new TagBuilder("div");
            row.AddCssClass("row");

            var prefixName = $"{prefix}[]";
            foreach (var node in nodes)
            {
                var render = await RenderProperties(helper, node, nodeProps, prefixName, setValue);
                if (render != null)
                    row.InnerHtml.AppendHtml(render);
            }

            cardBody.InnerHtml.AppendHtml(row);
            card.InnerHtml.AppendHtml(cardBody);

            bodyHtml.AppendHtml(card);
            i++;
        } while (i < existingPropsCount);


        return bodyHtml;
    }

    private IHtmlContent GetCardHeader(string prefix, string existingValue, int index, ContainerTokenType type)
    {
        var header = new TagBuilder("div");
        header.AddCssClass("card-header bg-light py-2");

        if (type == ContainerTokenType.Repeatable)
        {
            header.InnerHtml.AppendHtml(
                $"<div class='row'><div class='col-auto p-0'>{GetEnabledProperty(prefix, existingValue, false, index).GetString()}</div><div class='col rowIndex font-weight-bold'>{(index + 1)}</div><div class='col-auto'><button type='button' class='btn btn-sm btn-danger delete-row'><i class='fa fa-trash-o'></i></button></div></div>");
            // <div class='col-auto'><button type='button' class='btn btn-sm sort-row'><i class='fa fa-sort'></i></button></div>

            return header;
        }

        header.InnerHtml.AppendHtml(
            $"<div class='row'><div class='col p-0'>{GetEnabledProperty(prefix, existingValue, false, index).GetString()}</div></div>");
        // <div class='col-auto'><button type='button' class='btn btn-sm sort-row'><i class='fa fa-sort'></i></button></div>

        return header;
    }

    #endregion
}