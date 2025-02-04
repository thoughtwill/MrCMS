using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using MrCMS.Settings;

namespace MrCMS.AI.Settings;

public class OpenAiSettings : AiSettingsBase
{
    public OpenAiSettings()
    {
        TextModel = "gpt-4o";
        ImageModel = "dall-e-3";
        ApiKey = string.Empty;
        ImageSize = "1024x1024";
        ImageStyle = "natural";
        ImageQuality = "hd";
        ImageGenerationCount = 1;
    }

    public string TextModel { get; set; }

    public string ImageModel { get; set; }

    [DropDownSelection("AiImageQualityOptions")]
    public string ImageQuality { get; set; }

    [DropDownSelection("AiImageSizeOptions")]
    public string ImageSize { get; set; }

    [DropDownSelection("AiImageStyleOptions")]
    public string ImageStyle { get; set; }

    [DropDownSelection("AiImageGenerationCountOptions")]
    public int ImageGenerationCount { get; set; }

    public string ApiKey { get; set; }

    public override bool RenderInSettings => true;

    public override void SetViewData(IServiceProvider serviceProvider, ViewDataDictionary viewDataDictionary)
    {
        viewDataDictionary["AiImageQualityOptions"] = new List<SelectListItem>
        {
            new("High", "hd", ImageQuality == "hd"),
            new("Standard","standard", ImageQuality == "standard"),
        };
        viewDataDictionary["AiImageSizeOptions"] = new List<SelectListItem>
        {
            new("256x256", "256x256" , ImageSize == "256x256"),
            new("512x512", "512x512", ImageSize == "512x512"),
            new("1024x1024", "1024x1024", ImageSize == "1024x1024"),
            new("1024x1792", "1024x1792", ImageSize == "1024x1792"),
        };
        viewDataDictionary["AiImageStyleOptions"] = new List<SelectListItem>
        {
            new("Natural", "natural", ImageStyle == "natural"),
            new("Vivid", "vivid", ImageStyle == "vivid"),
        };
        viewDataDictionary["AiImageGenerationCountOptions"] = new List<SelectListItem>
        {
            new("1", "1", ImageGenerationCount == 1),
            new("2", "2", ImageGenerationCount == 2),
            new("4", "4", ImageGenerationCount == 4),
        };
    }
}