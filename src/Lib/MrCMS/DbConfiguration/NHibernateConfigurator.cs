using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentNHibernate.Automapping;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using MrCMS.Apps;
using MrCMS.Batching.Entities;
using MrCMS.DbConfiguration.Configuration;
using MrCMS.DbConfiguration.Conventions;
using MrCMS.DbConfiguration.Mapping;
using MrCMS.Entities;
using MrCMS.Entities.Documents;
using MrCMS.Entities.Documents.Web;
using MrCMS.Entities.Documents.Web.FormProperties;
using MrCMS.Entities.People;
using MrCMS.Entities.Widget;
using MrCMS.Helpers;
using NHibernate;
using NHibernate.Caches.CoreMemoryCache;
using NHibernate.Event;
using Environment = NHibernate.Cfg.Environment;

namespace MrCMS.DbConfiguration
{
    public class NHibernateConfigurator : IGetNHibernateConfiguration
    {
        private readonly IDatabaseProvider _databaseProvider;
        private readonly MrCMSAppContext _appContext;
        private readonly Action<CacheSettingsBuilder> _configureCache;
        private readonly string _connectionString;
        private Lazy<NHibernate.Cfg.Configuration> _configuration;

        public NHibernateConfigurator(IDatabaseProvider databaseProvider, MrCMSAppContext appContext,
            Action<CacheSettingsBuilder> configureCache = null, string connectionString = "")
        {
            _databaseProvider = databaseProvider;
            _appContext = appContext;
            _configureCache = configureCache;
            _connectionString = connectionString;
            _configuration = new Lazy<NHibernate.Cfg.Configuration>(LoadConfiguration);
        }

        public List<Assembly> ManuallyAddedAssemblies { get; set; }

        public ISessionFactory CreateSessionFactory()
        {
            var configuration = GetConfiguration();

            var sessionFactory = configuration.BuildSessionFactory();

            return sessionFactory;
        }

        public NHibernate.Cfg.Configuration GetConfiguration()
        {
            return _configuration.Value;
        }

        private NHibernate.Cfg.Configuration LoadConfiguration()
        {
            var assemblies = GetAssemblies();

            if (_databaseProvider == null)
                throw new Exception("Please set the database provider in mrcms.config");

            var iPersistenceConfigurer = _databaseProvider.GetPersistenceConfigurer();
            var autoPersistenceModel = GetAutoPersistenceModel(assemblies);

            var config = Fluently.Configure()
                .Database(iPersistenceConfigurer)
                .Mappings(m =>
                {
                    m.AutoMappings.Add(autoPersistenceModel);
                    foreach (var assembly in assemblies)
                        m.FluentMappings.AddFromAssembly(assembly);
                })
                .Cache(SetupCache)
                .ExposeConfiguration(AppendListeners)
                .ExposeConfiguration(AppSpecificConfiguration)
                .ExposeConfiguration(c =>
                {
#if DEBUG
                    c.SetProperty(Environment.GenerateStatistics, "true");
                    // c.DataBaseIntegration(x => { _databaseProvider.DebugDatabaseIntegration(x); });
#else
                    c.SetProperty(Environment.GenerateStatistics, "false");
#endif
                    c.SetProperty(Environment.Hbm2ddlKeyWords, "auto-quote");
                    c.SetProperty(Environment.BatchSize, "200");
                })
                .BuildConfiguration();


            _databaseProvider.AddProviderSpecificConfiguration(config);

            // ValidateSchema(config);

            //DbConfig.Initialize(_connectionString);

            config.BuildMappings();

            return config;
        }

        private List<Assembly> GetAssemblies()
        {
            var assemblies = TypeHelper.GetAllMrCMSAssemblies();
            if (ManuallyAddedAssemblies != null)
                assemblies.AddRange(ManuallyAddedAssemblies);

            var finalAssemblies = new List<Assembly>();

            assemblies.ForEach(assembly =>
            {
                if (finalAssemblies.All(a => a.FullName != assembly.FullName))
                    finalAssemblies.Add(assembly);
            });
            return finalAssemblies;
        }

        private void SetupCache(CacheSettingsBuilder builder)
        {
            if (_configureCache != null)
            {
                _configureCache(builder);
            }
            else
            {
                builder.UseSecondLevelCache().UseQueryCache().ProviderClass<CoreMemoryCacheProvider>();
            }
        }

        private AutoPersistenceModel GetAutoPersistenceModel(List<Assembly> assemblies)
        {
            return AutoMap.Assemblies(new MrCMSMappingConfiguration(), assemblies)
                .IgnoreBase<SystemEntity>()
                .IgnoreBase<SiteEntity>()
                .IncludeBase<Webpage>()
                .IncludeBase<UserProfileData>()
                .IncludeBase<Widget>()
                .IncludeBase<FormProperty>()
                .IncludeBase<FormPropertyWithOptions>()
                .IncludeBase<BatchJob>()
                .UseOverridesFromAssemblies(assemblies)
                .Conventions.AddFromAssemblyOf<CustomForeignKeyConvention>()
                .IncludeAppBases(_appContext)
                .IncludeAppConventions(_appContext);
        }

        private void AppSpecificConfiguration(NHibernate.Cfg.Configuration configuration)
        {
            _appContext.AppendConfiguration(configuration);
        }

        private void AppendListeners(NHibernate.Cfg.Configuration configuration)
        {
            configuration.AppendListeners(ListenerType.PreInsert, new[]
            {
                new SetCoreProperties()
            });
            var softDeleteListener = new SoftDeleteListener();
            configuration.SetListener(ListenerType.Delete, softDeleteListener);
        }
    }

    public interface IGetNHibernateConfiguration
    {
        NHibernate.Cfg.Configuration GetConfiguration();
    }
}