using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using MrCMS.AI.Services.Core;
using MrCMS.AI.Services.Providers;
using MrCMS.Helpers;
using MrCMS.Settings;

namespace MrCMS.AI.Settings;

public class AiSettings : AiSettingsBase
{

    public AiSettings()
    {
        EnhanceWebpageContentPromptTemplate = @"
You are an assistant within a [type of website] CMS. Enhance user-generated content by editing it for readability and SEO optimization. Treat the content as human-written and integrate SEO best practices.

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

        
        AiProvider = typeof(OllamaAiProvider).FullName;

    }
    
    [DropDownSelection("AiProviderOptions")]
    public string AiProvider { get; set; }

    [TextArea]
    public string EnhanceWebpageContentPromptTemplate { get; set; }
    
    [TextArea]
    public string GenerateWebpageSeoPromptTemplate { get; set; }
    
    [TextArea]
    public string GenerateWebpageBlocksPromptTemplate { get; set; }

    public override void SetViewData(IServiceProvider serviceProvider, ViewDataDictionary viewDataDictionary)
    {
        viewDataDictionary["AiProviderOptions"] = AiProviderOptions;
    }

    private List<SelectListItem> AiProviderOptions
    {
        get
        {
            var types = TypeHelper.GetAllConcreteTypesAssignableFrom<IAiProvider>();

            return types.BuildSelectItemList(type => type.Name.Replace("Provider", "").BreakUpString(),
                type => type.FullName, type => type.FullName == AiProvider,
                emptyItem: null);
        }
    }
    
    public override bool RenderInSettings => true;
}