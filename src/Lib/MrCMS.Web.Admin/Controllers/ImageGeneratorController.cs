using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MrCMS.AI.Services.Images;
using MrCMS.Web.Admin.Infrastructure.BaseControllers;

namespace MrCMS.Web.Admin.Controllers;

public class ImageGeneratorController: MrCMSAdminController
{
    private readonly GenerateImageAiService _generateImageAiService;

    public ImageGeneratorController(GenerateImageAiService generateImageAiService)
    {
        _generateImageAiService = generateImageAiService;
    }
    
    public async Task<IActionResult> Generate(string prompt, CancellationToken cancellationToken = default)
    {
        var generatedImageList = await _generateImageAiService.GenerateImageAsync(prompt, cancellationToken);
        return Json(generatedImageList);
    }
}