using System;
using System.Threading;
using System.Threading.Tasks;
using MrCMS.AI.Services.Images;
using MrCMS.Web.Admin.Infrastructure.BaseControllers;

namespace MrCMS.Web.Admin.Controllers;

public class ImageAiController: MrCMSAdminController
{
    private readonly GenerateImageAiService _generateImageAiService;

    public ImageAiController(GenerateImageAiService generateImageAiService)
    {
        _generateImageAiService = generateImageAiService;
    }
    
    public async Task<Uri> GenerateImage(string prompt, CancellationToken cancellationToken = default)
    {
        return await _generateImageAiService.GenerateImageAsync(prompt, cancellationToken);
    }
}