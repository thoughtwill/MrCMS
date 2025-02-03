using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using MrCMS.AI.Services.Core;
using MrCMS.AI.Services.Providers.Image;
using MrCMS.AI.Services.Providers.Text;
using MrCMS.Helpers;
using MrCMS.Settings;

namespace MrCMS.AI.Settings;

public class AiSettings : AiSettingsBase
{

    public AiSettings()
    {
        EnhanceWebpageContentPromptTemplate = @"
You are an assistant within a [type of website] CMS. Enhance user-generated content by editing it for readability and SEO optimization. Treat the content as human-written and integrate SEO best practices.
Additionally, incorporate standard HTML formatting (e.g., <p>, <h2>, <ul>, etc.) within the content to ensure proper structure and styling.

When crafting your response, please follow this format exactly , ensuring each tag starts and ends on a new line:
<title>
    <Suggested Title>
</title>
<content>
    <Enhanced Content with HTML tags>
</content>
Inputs to Guide Your Creation:
    Title: {{Title}}
    Body Content: {{Body Content}}
            ";
        
        GenerateWebpageSeoPromptTemplate = @"
You are an assistant within a CMS for a [type of website]. Your role is to optimize the content for search engines by generating relevant meta tags. 

When crafting your response, please follow this format exactly , ensuring each tag starts and ends on a new line:
<title>
    <SEO Title (for meta tag)>
</title>
<description>
    <SEO Description (for meta tag)>
</description>
<keywords>
    <SEO Keywords (for meta tag, separated by commas)>
</keywords>
Inputs to Guide Your Creation:
    Title: {{Title}}
    Body Content: {{Body Content}}
            ";

        GenerateWebpageBlocksPromptTemplate = @"
You are an assistant within a CMS for a [type of website]. Your task is to generate content based on the user's prompt and content token descriptions. Use the following inputs to guide your creation:

When crafting your response, please follow this format exactly , ensuring each tag starts and ends on a new line:
<block>
    <block in this format: { Name: 'Block Name', Content: 'HTML content utilizing tokens' }>
</block>
<block>
    <block in this format: { Name: 'Block Name', Content: 'HTML content utilizing tokens' }>
</block>
repeat for each block as needed

Inputs to guide your creation:
    User Prompt: {{User Prompt}}
    Content Token Description: {{Content Token Description}}
            ";

        
        AiTextProvider = typeof(OllamaAiTextProvider).FullName;
        AiImageProvider = typeof(OpenAiImageProvider).FullName;

    }
    
    [DropDownSelection("AiTextProviderOptions")]
    public string AiTextProvider { get; set; }
    
    [DropDownSelection("AiImageProviderOptions")]
    public string AiImageProvider { get; set; }

    [TextArea]
    public string EnhanceWebpageContentPromptTemplate { get; set; }
    
    [TextArea]
    public string GenerateWebpageSeoPromptTemplate { get; set; }
    
    [TextArea]
    public string GenerateWebpageBlocksPromptTemplate { get; set; }

    public override void SetViewData(IServiceProvider serviceProvider, ViewDataDictionary viewDataDictionary)
    {
        viewDataDictionary["AiTextProviderOptions"] = AiTextProviderOptions;
        viewDataDictionary["AiImageProviderOptions"] = AiImageProviderOptions;
    }

    private List<SelectListItem> AiTextProviderOptions
    {
        get
        {
            var types = TypeHelper.GetAllConcreteTypesAssignableFrom<IAiTextProvider>();

            return types.BuildSelectItemList(type => type.Name.Replace("TextProvider", "").Replace("Provider", "").BreakUpString(),
                type => type.FullName, type => type.FullName == AiTextProvider,
                emptyItem: null);
        }
    }

    private List<SelectListItem> AiImageProviderOptions
    {
        get
        {
            var types = TypeHelper.GetAllConcreteTypesAssignableFrom<IAiImageProvider>();

            return types.BuildSelectItemList(type => type.Name.Replace("ImageProvider", "").Replace("Provider", "").BreakUpString(),
                type => type.FullName, type => type.FullName == AiTextProvider,
                emptyItem: null);
        }
    }
    
    public override bool RenderInSettings => true;
}