using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using MrCMS.Helpers;
using MrCMS.Models;
using MrCMS.Settings;

namespace MrCMS.Services
{
    public class AzureFileSystem : IAzureFileSystem
    {
        // private readonly IConfigurationProvider _configurationProvider;
        private BlobContainerClient _containerClient;
        private BlobServiceClient _serviceClient;
        private bool _initialized;
        private readonly FileSystemSettings _settings;
        private readonly ILogger<AzureFileSystem> _logger;

        public AzureFileSystem(FileSystemSettings settings, ILogger<AzureFileSystem> logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public async Task<BlobContainerClient> GetContainerClient()
        {
            await EnsureInitialized();
            return _containerClient;
        }

        public async Task<BlobServiceClient> GetServiceClient()
        {
            await EnsureInitialized();
            return _serviceClient;
        }

        public async Task<string> SaveFile(Stream stream, string filePath, string contentType)
        {
            await EnsureInitialized();
            var blob = await GetBlobClient(filePath);

            if (await blob.ExistsAsync())
                return blob.Uri.ToString();

            if (stream.CanSeek)
            {
                stream.Position = 0;
            }

            try
            {
                await blob.UploadAsync(stream);

                await blob.SetHttpHeadersAsync(new BlobHttpHeaders
                    {ContentType = contentType, CacheControl = "public, max-age=31536000"});
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not save file: {Uri}", blob.Uri);
            }

            return blob.Uri.ToString();
        }

        public Task CreateDirectory(string filePath)
        {
            return Task.CompletedTask;
        }

        public async Task Delete(string filePath)
        {
            await EnsureInitialized();
            var blob = await GetBlobClient(filePath);
            try
            {
                await blob.DeleteAsync();
            }
            catch
            {
                // ignored
            }
        }

        public async Task<bool> Exists(string filePath)
        {
            await EnsureInitialized();
            if (string.IsNullOrWhiteSpace(filePath))
                return false;
            var blob = await GetBlobClient(filePath);

            return await blob.ExistsAsync();
        }

        public async Task<byte[]> ReadAllBytes(string filePath)
        {
            await EnsureInitialized();
            var blob = await GetBlobClient(filePath);
            await using var memoryStream = new MemoryStream();
            await blob.DownloadToAsync(memoryStream);
            return memoryStream.ToArray();
        }

        public async Task<Stream> GetReadStream(string filePath)
        {
            await EnsureInitialized();
            var blob = await GetBlobClient(filePath);
            return await blob.OpenReadAsync();
        }

        public async Task WriteToStream(string filePath, Stream stream)
        {
            await EnsureInitialized();
            var blob = await GetBlobClient(filePath);
            await blob.DownloadToAsync(stream);
        }

        public async Task<IEnumerable<string>> GetFiles(string filePath)
        {
            await EnsureInitialized();
            var container = await GetContainerClient();
            var cloudBlobDirectory = container.GetBlobsByHierarchyAsync(prefix: filePath);

            // var blobResultSegment = await cloudBlobDirectory
            //     .ListBlobsSegmentedAsync(true, BlobListingDetails.None, 500, null, null, null);

            List<string> files = new List<string>();
            // var continuationToken = blobResultSegment.ContinuationToken;
            // files.AddRange(blobResultSegment.Results.Select(item => item.Uri.ToString()));

            await foreach (var page in cloudBlobDirectory)
            {
                files.Add(Path.Combine(container.Uri.AbsoluteUri, page.Blob.Name));
            }

            return files;
        }

        public async Task<CdnInfo> GetCdnInfo()
        {
            // var fileSystemSettings = _configurationProvider.GetSiteSettings<FileSystemSettings>();
            if (!string.IsNullOrWhiteSpace(_settings.AzureCdnDomain))
            {
                var cdnInfo = new CdnInfo
                {
                    Scheme = "https",
                    Host = _settings.AzureCdnDomain
                };
                return await Task.FromResult(cdnInfo);
            }

            return null;
        }

        private async Task EnsureInitialized()
        {
            if (_initialized)
            {
                return;
            }

            // var fileSystemSettings = _configurationProvider.GetSiteSettings<FileSystemSettings>();
            string connectionString = _settings.AzureUsingEmulator
                ? "UseDevelopmentStorage=true;"
                : $"DefaultEndpointsProtocol=https;AccountName={_settings.AzureAccountName};AccountKey={_settings.AzureAccountKey}";
            _serviceClient = new BlobServiceClient(connectionString);
            var containerName = SeoHelper.TidyUrl(_settings.AzureContainerName.RemoveInvalidUrlCharacters());
            
            _containerClient = _serviceClient.GetBlobContainerClient(containerName);
            await _containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            _initialized = true;
        }

        public async Task<BlobClient> GetBlobClient(string filePath)
        {
            var container = await GetContainerClient();
            var uri = container.Uri;
            filePath = filePath.Replace(uri + "/", "");
            return container.GetBlobClient(filePath);
        }
    }
}