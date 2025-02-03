using System.Threading;
using System.Threading.Tasks;
using MrCMS.AI.Models;

namespace MrCMS.AI.Services.Core;

public interface IAiImageProvider
{
    /// <summary>
    /// Generates an image based on the provided prompt.
    /// </summary>
    /// <param name="prompt">The text prompt for the image.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an ImageResponse.</returns>
    Task<AiImageResponse> GenerateImageAsync(string prompt, CancellationToken cancellationToken = default);
}