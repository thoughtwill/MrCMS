using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using MrCMS.DbConfiguration;
using MrCMS.Entities.Documents.Media;
using MrCMS.Helpers;
using MrCMS.Models;
using MrCMS.Settings;
using MrCMS.Website.Caching;
using NHibernate;


namespace MrCMS.Services
{
    public class ImageRenderingService : IImageRenderingService
    {
        private readonly ICacheManager _cacheManager;
        private readonly IOptions<ExternalImageResizeConfig> _externalImageResizeConfig;
        private readonly IFileService _fileService;
        private readonly IImageProcessor _imageProcessor;
        private readonly MediaSettings _mediaSettings;
        private readonly ISession _session;

        public ImageRenderingService(ISession session, IImageProcessor imageProcessor, IFileService fileService,
            MediaSettings mediaSettings, ICacheManager cacheManager, IOptions<ExternalImageResizeConfig> externalImageResizeConfig)
        {
            _session = session;
            _imageProcessor = imageProcessor;
            _fileService = fileService;
            _mediaSettings = mediaSettings;
            _cacheManager = cacheManager;
            _externalImageResizeConfig = externalImageResizeConfig;
        }

        public async Task<ImageInfo> GetImageInfo(string imageUrl, Size targetSize)
        {
            var image = await _imageProcessor.GetImage(imageUrl);
            if (image != null)
                return new ImageInfo
                {
                    Title = image.Title,
                    Description = image.Description,
                    ImageUrl = await GetFileImageUrl(image, targetSize),
                    ActualSize = ImageProcessor.CalculateDimensions(image.Size, targetSize)
                };
            return null;
        }

        public async Task<string> GetImageUrl(string imageUrl, Size targetSize)
        {
            var info = _mediaSettings.GetImageUrlCachingInfo(imageUrl, targetSize);
            return await _cacheManager.GetOrCreateAsync(info.CacheKey, async () =>
            {
                var imageInfo = new ImageInfo();
                if (_externalImageResizeConfig.Value.Enabled)
                {
                    imageInfo.ImageUrl = GenerateResizedImageUrl(imageUrl, targetSize.Width, targetSize.Height);
                }
                else
                {
                    imageInfo = await GetImageInfo(imageUrl, targetSize);
                }

                return imageInfo?.ImageUrl;
            }, info.TimeToCache, info.ExpiryType);
        }

        public async Task<IHtmlContent> RenderImage(IHtmlHelper helper, string imageUrl, Size targetSize = new Size(),
            string alt = null,
            string title = null, bool enableCaption = false, object attributes = null,
            bool showPlaceholderIfNull = false, bool enableLazyLoading = true)
        {
            var cachingInfo = _mediaSettings.GetImageTagCachingInfo(imageUrl, targetSize, alt, title, enableCaption,
                enableLazyLoading, attributes);

            return await _cacheManager.GetOrCreateAsync(cachingInfo.CacheKey, async () =>
            {

                using (new SiteFilterDisabler(_session))
                {
                    if (string.IsNullOrWhiteSpace(imageUrl) && showPlaceholderIfNull &&
                        !string.IsNullOrEmpty(_mediaSettings.HoldingImage))
                    {
                        return ReturnTag(await GetImageInfo(_mediaSettings.HoldingImage, targetSize), alt, title,
                            enableCaption, enableLazyLoading, attributes);
                    }

                    if (string.IsNullOrWhiteSpace(imageUrl))
                    {
                        return HtmlString.Empty;
                    }

                    var imageInfo = new ImageInfo();
                    if (_externalImageResizeConfig.Value.Enabled)
                    {
                        imageInfo.ImageUrl = GenerateResizedImageUrl(imageUrl, targetSize.Width, targetSize.Height);
                        //todo: we could get alt/title inline like this, but it's not ideal as n+1. We should load and pass it in, or make it opt in with bool lookupMetaData = true, uses for article pages but not listings
                        /*if (alt == null || title == null)
                        {
                            var (metaTitle, metaDescription) = await _imageProcessor.GetImageMetaData(imageUrl);
                            imageInfo.Title = metaTitle;
                            imageInfo.Description = metaDescription;
                        }
                        else
                        {*/
                            imageInfo.Title = title;
                            imageInfo.Description = alt;
                        /*}*/

                    }
                    else
                    {
                        imageInfo = await GetImageInfo(imageUrl, targetSize);
                        if (imageInfo == null)
                            return HtmlString.Empty;
                    }

                    return ReturnTag(imageInfo, alt, title, enableCaption, enableLazyLoading, attributes);
                }
            }, cachingInfo.TimeToCache, cachingInfo.ExpiryType);
        }

        private async Task<string> GetFileImageUrl(MediaFile image, Size targetSize)
        {
            return await _fileService.GetFileLocation(image, targetSize, true);
        }

        private async Task<string> GetCropImageUrl(Crop crop, Size targetSize)
        {
            return await _fileService.GetFileLocation(crop, targetSize, true);
        }


        private IHtmlContent ReturnTag(ImageInfo imageInfo, string alt, string title, bool enableCaption,
            bool enableLazyLoading, object attributes)
        {
            var tagBuilder = new TagBuilder("img");
            tagBuilder.Attributes.Add("src", imageInfo.ImageUrl);
            tagBuilder.Attributes.Add("alt", alt ?? imageInfo.Title);
            tagBuilder.Attributes.Add("title", title ?? imageInfo.Description);
            if (enableLazyLoading)
            {
                tagBuilder.Attributes.Add("loading", "lazy");
            }

            if (attributes != null)
            {
                var routeValueDictionary = MrCMSHtmlHelperExtensions.AnonymousObjectToHtmlAttributes(attributes);
                foreach (var kvp in routeValueDictionary.Where(kvp => kvp.Value != null))
                    tagBuilder.Attributes.Add(kvp.Key, kvp.Value!.ToString());
            }

            tagBuilder.TagRenderMode = TagRenderMode.SelfClosing;

            if (enableCaption && !string.IsNullOrWhiteSpace(imageInfo.Description))
            {
                var figureBuilder = new TagBuilder("figure");
                figureBuilder.Attributes.Add("class", "figure");
                var figCaptionBuilder = new TagBuilder("figCaption");
                figCaptionBuilder.Attributes.Add("class", "figure-caption py-2");
                figCaptionBuilder.InnerHtml.Append(imageInfo.Description);

                figureBuilder.InnerHtml.AppendHtml(tagBuilder);
                figureBuilder.InnerHtml.AppendHtml(figCaptionBuilder);

                return figureBuilder;
            }

            return tagBuilder;
        }

        private string GenerateResizedImageUrl(string url, int width, int height, int? quality = null)
        {
            if (string.IsNullOrWhiteSpace(url))
                return string.Empty;

            if (height == 0)
                height = width;
            if (width == 0)
                width = height;

            var absolutePath = new Uri(_imageProcessor.GetOriginalImageUrl(url)).AbsolutePath;
            var urlForImage = height == 0 && width == 0 ? "/original" : $"/thumbs/{width}x{height}";

            var uriBuilder = new UriBuilder(url)
            {
                Host = _externalImageResizeConfig.Value.Host,
                Path = urlForImage + absolutePath
            };

            if (!quality.HasValue) return uriBuilder.ToString();

            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["quality"] = quality.Value.ToString();
            uriBuilder.Query = query.ToString() ?? string.Empty;

            return uriBuilder.ToString();
        }
    }
}
