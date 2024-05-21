using System;
using System.Configuration;
using FluentNHibernate.Cfg.Db;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MrCMS.Apps;
using MrCMS.DbConfiguration;
using MrCMS.Settings;
using NHibernate;
using ISession = NHibernate.ISession;

namespace MrCMS.Helpers
{
    public static class MrCmsDataExtensions
    {
        public static IServiceCollection AddMrCMSData(this IServiceCollection services, bool isInstalled,
            IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            services.AddScoped<ISession>(provider =>
            {
                var session = provider.GetRequiredService<IHttpContextAccessor>()?.HttpContext
                    ?.Items?["override-nh-session"] as ISession;
                return session ?? provider.GetRequiredService<ISessionFactory>().OpenFilteredSession(provider);
            });
            services.AddTransient<IStatelessSession>(provider =>
            {
                var session =
                    provider.GetRequiredService<IHttpContextAccessor>()?.HttpContext
                        ?.Items?["override-nh-stateless-session"] as IStatelessSession;
                return session ?? provider.GetRequiredService<ISessionFactory>().OpenStatelessSession();
            });

            if (isInstalled)
            {
                var connectionString = configuration.GetConnectionString("mrcms");
                services.Configure<DatabaseSettings>(settings =>
                {
                    settings.ConnectionString = connectionString;
                    var section = configuration.GetSection("Database");
                    settings.DatabaseProviderType =
                        section.GetValue<string>(nameof(DatabaseSettings.DatabaseProviderType));
                    settings.LogQueries = section.GetValue<bool>(nameof(DatabaseSettings.LogQueries));
                });

                services.RegisterDatabaseProviders();

                services.AddSingleton<IDatabaseProviderResolver, DatabaseProviderResolver>();


                services.AddSingleton<NHibernateConfigurator>(provider =>
                {
                    Action<CacheSettingsBuilder> configureCache;
                    if (webHostEnvironment.EnvironmentName == "Development")
                    {
                        configureCache = x => x.Not.UseQueryCache().Not.UseSecondLevelCache();
                    }
                    else
                    {
                        configureCache = null; //default configure
                    }
                    return new NHibernateConfigurator(
                        provider.GetRequiredService<IDatabaseProviderResolver>().GetProvider(),
                        provider.GetRequiredService<MrCMSAppContext>(),
                        configureCache, connectionString);
                });

                services.AddSingleton<ISessionFactory>(provider =>
                    provider.GetRequiredService<NHibernateConfigurator>().CreateSessionFactory());
                services.AddSingleton<IGetNHibernateConfiguration>(provider => provider.GetRequiredService<NHibernateConfigurator>());
            }

            return services;
        }
    }
}