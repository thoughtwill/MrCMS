using Microsoft.Extensions.DependencyInjection;
using MrCMS.DbConfiguration;
using MrCMS.Entities.Documents.Web.FormProperties;
using MrCMS.Services;
using MrCMS.Settings;
using MrCMS.Shortcodes.Forms;
using MrCMS.Tasks;
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Routing;
using MrCMS.Entities.Documents.Metadata;
using MrCMS.Entities.Documents.Web;
using MrCMS.Shortcodes;

namespace MrCMS.Helpers
{
    public static class IoCRegistrationExtensions
    {
        public static void RegisterAllSimplePairings(this IServiceCollection container)
        {
            var pairings = TypeHelper.GetSimpleInterfaceImplementationPairings();
            foreach (var interfaceType in pairings.Keys)
            {
                if (!container.Any(x =>
                        x.ServiceType == interfaceType && x.ImplementationType == pairings[interfaceType]))
                    container.AddScoped(interfaceType, pairings[interfaceType]);
            }
        }

        public static void RegisterOpenGenerics(this IServiceCollection container)
        {
            var interfaces = TypeHelper.GetAllOpenGenericInterfaces();

            foreach (var interfaceType in interfaces)
            {
                foreach (var type in TypeHelper.GetAllConcreteTypesAssignableFromGeneric(interfaceType)
                             .Where(x => x.IsGenericTypeDefinition))
                {
                    container.AddScoped(interfaceType, type);
                }
            }
        }

        public static void SelfRegisterAllConcreteTypes(this IServiceCollection container)
        {
            foreach (var type in TypeHelper.GetAllConcreteTypesAssignableFrom<object>())
            {
                if (container.All(x => x.ServiceType != type))
                    container.AddScoped(type);
            }
        }

        public static void RegisterSiteLocator(this IServiceCollection container)
        {
            container.AddScoped<ICurrentSiteLocator, ContextCurrentSiteLocator>();
        }

        public static void RegisterSettings(this IServiceCollection container)
        {
            foreach (var type in TypeHelper.GetAllConcreteTypesAssignableFrom<SystemSettingsBase>())
            {
                container.AddScoped(type,
                    provider =>
                    {
                        var configurationProvider = provider.GetRequiredService<ISystemConfigurationProvider>();
                        var methodInfo = configurationProvider.GetType()
                            .GetMethodExt(nameof(ISystemConfigurationProvider.GetSystemSettings));
                        return methodInfo.MakeGenericMethod(type).Invoke(configurationProvider, Array.Empty<object>());
                    });
            }

            foreach (var type in TypeHelper.GetAllConcreteTypesAssignableFrom<SiteSettingsBase>())
            {
                container.AddScoped(type,
                    provider =>
                    {
                        var configurationProvider = provider.GetRequiredService<IConfigurationProvider>();
                        var methodInfo = configurationProvider.GetType()
                            .GetMethodExt(nameof(IConfigurationProvider.GetSiteSettings));
                        return methodInfo.MakeGenericMethod(type).Invoke(configurationProvider, Array.Empty<object>());
                    });
            }
        }

        public static void RegisterFormRenderers(this IServiceCollection container)
        {
            var allRenderers = TypeHelper.GetAllConcreteTypesAssignableFromGeneric(typeof(IFormElementRenderer<>));
            foreach (var type in TypeHelper.GetAllConcreteTypesAssignableFrom<FormProperty>())
            {
                var rendererInterfaceType = typeof(IFormElementRenderer<>).MakeGenericType(type);
                var concreteType = allRenderers.FirstOrDefault(x => rendererInterfaceType.IsAssignableFrom(x));
                if (concreteType != null)
                {
                    container.AddScoped(rendererInterfaceType, concreteType);
                }
            }
        }

        public static void RegisterShortCodeRenderers(this IServiceCollection container)
        {
            foreach (var type in TypeHelper.GetAllConcreteTypesAssignableFrom<IShortcodeRenderer>())
            {
                // don't double add 
                if (!container.Any(x => x.ServiceType == typeof(IShortcodeRenderer) && x.ImplementationType == type))
                    container.AddScoped(typeof(IShortcodeRenderer), type);
            }
        }

        public static void RegisterRouteTransformers(this IServiceCollection container)
        {
            foreach (var type in TypeHelper.GetAllConcreteTypesAssignableFrom<DynamicRouteValueTransformer>())
            {
                container.AddTransient(type);
            }
        }

        public static void RegisterTokenProviders(this IServiceCollection container)
        {
            foreach (var type in TypeHelper.GetAllConcreteTypesAssignableFrom<ITokenProvider>())
            {
                if (!container.Any(x => x.ServiceType == typeof(ITokenProvider) && x.ImplementationType == type))
                    container.AddScoped(typeof(ITokenProvider), type);
            }

            var tokenProviderGenericType = typeof(ITokenProvider<>);
            var tokenProviderTypes = TypeHelper.GetAllConcreteTypesAssignableFromGeneric(tokenProviderGenericType);
            foreach (var type in tokenProviderTypes)
            {
                if (type.IsGenericType)
                {
                    if (!container.Any(x => x.ServiceType == tokenProviderGenericType && x.ImplementationType == type))
                        container.AddScoped(tokenProviderGenericType, type);
                }
                else
                {
                    var typed = type.GetBaseTypes(true).SelectMany(x => x.GetInterfaces())
                        .FirstOrDefault(x => x.GetGenericTypeDefinition() == tokenProviderGenericType);
                    if (typed != null && !container.Any(x => x.ServiceType == typed && x.ImplementationType == type))
                    {
                        container.AddScoped(typed, type);
                    }
                }
            }
        }

        public static void RegisterWebpageMetadata(this IServiceCollection container)
        {
            foreach (var type in TypeHelper.GetAllConcreteTypesAssignableFrom<IGetWebpageMetadataInfo>())
            {
                if (!type.IsGenericType)
                {
                    container.AddSingleton(typeof(IGetWebpageMetadataInfo), type);
                    container.AddSingleton(type, type);
                }
                else
                {
                    foreach (
                        var webpageType in
                        TypeHelper.GetAllConcreteMappedClassesAssignableFrom<Webpage>()
                            .Where(type => !type.ContainsGenericParameters))
                    {
                        var genericType = type.MakeGenericType(webpageType);
                        container.AddSingleton(genericType, genericType);
                    }
                }
            }
        }

        public static void RegisterDatabaseProviders(this IServiceCollection container)
        {
            foreach (var type in TypeHelper.GetAllConcreteTypesAssignableFrom<IDatabaseProvider>())
            {
                container.AddTransient(typeof(IDatabaseProvider), type);
                container.AddTransient(type);
            }
        }
    }
}