using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MrCMS.Batching;
using MrCMS.Batching.Services;
using MrCMS.Entities.Documents.Media;
using MrCMS.Helpers;
using MrCMS.Settings;
using Newtonsoft.Json;
using NHibernate;

namespace MrCMS.Services.FileMigration
{
    public class FileMigrationService : IFileMigrationService
    {
        private readonly Dictionary<string, IFileSystem> _allFileSystems;
        private readonly ICreateBatch _createBatch;
        private readonly FileSystemSettings _fileSystemSettings;
        private readonly IServiceProvider _serviceProvider;
        private readonly ISession _session;
        private readonly IUrlHelper _urlHelper;

        public FileMigrationService(IServiceProvider serviceProvider, FileSystemSettings fileSystemSettings,
            ISession session,
            ICreateBatch createBatch, IUrlHelper urlHelper)
        {
            IEnumerable<IFileSystem> fileSystems = TypeHelper.GetAllConcreteTypesAssignableFrom<IFileSystem>()
                .Select(type => serviceProvider.GetService(type) as IFileSystem);
            _allFileSystems =
                fileSystems
                    .ToDictionary(system => system.GetType().FullName);
            _fileSystemSettings = fileSystemSettings;
            _session = session;
            _createBatch = createBatch;
            _serviceProvider = serviceProvider;
            _urlHelper = urlHelper;
        }

        public IFileSystem CurrentFileSystem
        {
            get
            {
                string storageType = _fileSystemSettings.StorageType;
                return _allFileSystems[storageType];
            }
        }

        public async Task<FileMigrationResult> MigrateFiles()
        {
            IList<MediaFile> mediaFiles = await _session.QueryOver<MediaFile>().ListAsync();

            List<Guid> guids = new List<Guid>();
            foreach (var file in mediaFiles)
            {
                if (await MediaFileExtensions.GetFileSystem(file, _allFileSystems.Values) != CurrentFileSystem)
                {
                    guids.Add(file.Guid);
                }
            }

            if (!guids.Any())
            {
                return new FileMigrationResult
                {
                    MigrationRequired = false,
                    Message = "Migration not required"
                };
            }

            BatchCreationResult result = await _createBatch.Create(guids.Chunk(10)
                .Select(set => new MigrateFilesBatchJob
                {
                    Data = JsonConvert.SerializeObject(set.ToHashSet()),
                }));

            return new FileMigrationResult
            {
                MigrationRequired = true,
                Message =
                    await "Batch created. Click <a target=\"_blank\" href=\"{url}\">here</a> to view and start."
                        .AsResource(_serviceProvider,
                            configureOptions => configureOptions.AddReplacement("url",
                                _urlHelper.Action("Show", "BatchRun", new { id = result.InitialBatchRun.Id })))
            };
        }
    }

    public class FileMigrationResult
    {
        public bool MigrationRequired { get; set; }
        public string Message { get; set; }
    }
}