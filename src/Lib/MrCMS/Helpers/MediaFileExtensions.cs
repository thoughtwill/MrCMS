using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using MrCMS.Entities.Documents.Media;
using MrCMS.Models;
using MrCMS.Services;
using MrCMS.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MrCMS.Helpers
{
    public static class MediaFileExtensions
    {
        public static readonly HashSet<string> JpegExtensions = [ ".jpg", ".jpeg" ];
        public static readonly List<string> ImageExtensions = [".jpg", ".jpeg", ".gif", ".png", ".bmp", ".webp"];
        public static readonly List<string> VectorImageExtensions = [".svg"];

        public static async Task<IFileSystem> GetFileSystem(MediaFile file, IEnumerable<IFileSystem> possibleFileSystems)
        {
            return await GetFileSystem(file?.FileUrl, possibleFileSystems);
        }
        public static async Task<IFileSystem> GetFileSystem(string fileUrl, IEnumerable<IFileSystem> possibleFileSystems)
        {
            foreach (var fileSystem in possibleFileSystems)
            {
                if (await fileSystem.Exists(fileUrl))
                    return fileSystem;
            }

            return null;
        }

        public static bool IsImage(this MediaFile file)
        {
            return file != null && IsImageExtension(file.FileExtension);
        }

        public static bool IsImageExtension(string fileExtension)
        {
            return ImageExtensions.Any(s => s.Equals(fileExtension, StringComparison.InvariantCultureIgnoreCase));
        }
        
        public static bool IsVectorImage(this MediaFile file)
        {
            return file != null && IsVectorImageExtension(file.FileExtension);
        }
        
        public static bool IsVectorImageExtension(string fileExtension)
        {
            return VectorImageExtensions.Any(s => s.Equals(fileExtension, StringComparison.InvariantCultureIgnoreCase));
        }

        public static bool IsJpeg(this MediaFile file)
        {
            return file != null &&
                   JpegExtensions.Any(s => s.Equals(file.FileExtension, StringComparison.InvariantCultureIgnoreCase));
        }

        public static IEnumerable<ImageSize> GetSizes(this MediaFile file, MediaSettings mediaSettings)
        {
            if (!IsImage(file))
            {
                yield break;
            }

            yield return new ImageSize("Original", file.Size);
            foreach (ImageSize imageSize in
                mediaSettings.ImageSizes.Where(size => ImageProcessor.RequiresResize(file.Size, size.Size)))
            {
                imageSize.ActualSize = ImageProcessor.CalculateDimensions(file.Size, imageSize.Size);
                yield return imageSize;
            }
        }

        public static IEnumerable<ImageSize> GetImageSizes(this IHtmlHelper helper, MediaFile file)
        {
            return GetSizes(file, helper.ViewContext.HttpContext.RequestServices.GetRequiredService<MediaSettings>());
        }
    }
}