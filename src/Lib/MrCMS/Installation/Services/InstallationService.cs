using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MrCMS.Apps;
using MrCMS.DbConfiguration;
using MrCMS.Entities.Multisite;
using MrCMS.Helpers;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MrCMS.Installation.Models;
using ISession = NHibernate.ISession;

namespace MrCMS.Installation.Services
{
    public class InstallationService : IInstallationService
    {
        private readonly IDatabaseCreationService _databaseCreationService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IFileSystemAccessService _fileSystemAccessService;

        public InstallationService(IFileSystemAccessService fileSystemAccessService, IDatabaseCreationService databaseCreationService, IServiceProvider serviceProvider)
        {
            _fileSystemAccessService = fileSystemAccessService;
            _databaseCreationService = databaseCreationService;
            _serviceProvider = serviceProvider;
        }

        public async Task<InstallationResult> Install(InstallModel model)
        {
            if (model.DatabaseConnectionString != null)
            {
                model.DatabaseConnectionString = model.DatabaseConnectionString.Trim();
            }

            InstallationResult result = _databaseCreationService.ValidateConnectionString(model);
            if (!result.Success)
            {
                return result;
            }

            result = _fileSystemAccessService.EnsureAccessToFileSystem();
            if (!result.Success)
            {
                return result;
            }

            _fileSystemAccessService.EmptyAppData();

            try
            {
                IDatabaseProvider provider = _databaseCreationService.CreateDatabase(model);

                //save settings
                await SetUpInitialData(model, provider);
            }
            catch (Exception exception)
            {
                result.AddModelError("Setup failed: " + exception);
                _fileSystemAccessService.EmptyAppData();
            }

            return result;
        }

        public List<DatabaseProviderInfo> GetProviderTypes()
        {
            return TypeHelper.GetAllConcreteTypesAssignableFrom<IDatabaseProvider>()
                .Select(type => new DatabaseProviderInfo(type)).ToList();
        }

        public bool DatabaseIsInstalled()
        {
            return _databaseCreationService.IsDatabaseInstalled();
        }

        private async Task SetUpInitialData(InstallModel model, IDatabaseProvider provider)
        {
            var configurator =
                new NHibernateConfigurator(provider, _serviceProvider.GetRequiredService<MrCMSAppContext>());

            var validateNHibernateSchema = new ValidateNHibernateSchema(configurator);
            await validateNHibernateSchema.Execute(CancellationToken.None);
            
            ISessionFactory sessionFactory = configurator.CreateSessionFactory();
            ISession session = sessionFactory.OpenFilteredSession(_serviceProvider);
            IStatelessSession statelessSession = sessionFactory.OpenStatelessSession();
            
            var contextAccessor = _serviceProvider.GetRequiredService<IHttpContextAccessor>();
            var context = contextAccessor.HttpContext;
            context.Items["override-nh-session"] = session;
            context.Items["override-nh-stateless-session"] = statelessSession;
            var site = new Site
            {
                Name = model.SiteName,
                BaseUrl = model.SiteUrl,
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow
            };
            using (ITransaction transaction = statelessSession.BeginTransaction())
            {
                await statelessSession.InsertAsync(site);
                await transaction.CommitAsync();
            }
            context.Items["override-site"] = site;
            //CurrentRequestData.CurrentSite = site;

            await _serviceProvider.GetRequiredService<IInitializeDatabase>().Initialize(model);
            await _serviceProvider.GetRequiredService<ICreateInitialUser>().Create(model);
            var installations = _serviceProvider.GetServices<IOnInstallation>()
                .OrderBy(installation => installation.Priority);
            foreach (var installation in installations)
            {
                await installation.Install(model);
            }
        }
    }
}