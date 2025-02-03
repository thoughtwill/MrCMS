using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using MrCMS.Settings;
using OpenAI.Images;

namespace MrCMS.AI.Settings;

public class OpenAiSettings : AiSettingsBase
{
    public OpenAiSettings()
    {
        TextModel = "gpt-4o";
        ImageModel = "dall-e-3";
        ApiKey = string.Empty;
        ImageSize = GeneratedImageSize.W1024xH1024;
        ImageStyle = GeneratedImageStyle.Natural;
        ImageQuality = GeneratedImageQuality.High;
        ImageGenerationCount = 1;
    }

    public string TextModel { get; set; }

    public string ImageModel { get; set; }

    [DropDownSelection("AiImageQualityOptions")]
    public GeneratedImageQuality ImageQuality { get; set; }

    [DropDownSelection("AiImageSizeOptions")]
    public GeneratedImageSize ImageSize { get; set; }

    [DropDownSelection("AiImageStyleOptions")]
    public GeneratedImageStyle ImageStyle { get; set; }

    [DropDownSelection("AiImageGenerationCountOptions")]
    public int ImageGenerationCount { get; set; }

    public string ApiKey { get; set; }

    public override bool RenderInSettings => true;

    public override void SetViewData(IServiceProvider serviceProvider, ViewDataDictionary viewDataDictionary)
    {
        viewDataDictionary["AiImageQualityOptions"] = new List<SelectListItem>
        {
            new("High", GeneratedImageQuality.High.ToString())
        };
        viewDataDictionary["AiImageSizeOptions"] = new List<SelectListItem>
        {
            new("256x256", GeneratedImageSize.W256xH256.ToString()),
            new("512x512", GeneratedImageSize.W512xH512.ToString()),
            new("1024x1024", GeneratedImageSize.W1024xH1024.ToString()),
            new("1024x1792", GeneratedImageSize.W1024xH1792.ToString())
        };
        viewDataDictionary["AiImageStyleOptions"] = new List<SelectListItem>
        {
            new("Natural", GeneratedImageStyle.Natural.ToString()),
            new("Vivid", GeneratedImageStyle.Vivid.ToString())
        };
        viewDataDictionary["AiImageGenerationCountOptions"] = new List<SelectListItem>
        {
            new("1", "1"),
            new("2", "2"),
            new("4", "4")
        };
    }
}