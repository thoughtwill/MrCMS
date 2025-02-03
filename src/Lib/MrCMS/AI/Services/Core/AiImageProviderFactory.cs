using System;
using MrCMS.AI.Settings;

namespace MrCMS.AI.Services.Core;

public interface IAiImageProviderFactory
{
    IAiImageProvider GetProvider();
}

public class AiImageProviderFactory : IAiImageProviderFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly AiSettings _settings;

    public AiImageProviderFactory(IServiceProvider serviceProvider, AiSettings settings)
    {
        _serviceProvider = serviceProvider;
        _settings = settings;
    }
    
    public IAiImageProvider GetProvider()
    {
        var providerType = Type.GetType(_settings.AiImageProvider);

        if (providerType == null || !typeof(IAiImageProvider).IsAssignableFrom(providerType))
        {
            throw new InvalidOperationException($"Invalid or unsupported AI provider type: {_settings.AiImageProvider}");
        }

        if (_serviceProvider.GetService(providerType) is not IAiImageProvider provider)
        {
            throw new InvalidOperationException($"Failed to resolve AI provider: {_settings.AiImageProvider}");
        }

        return provider;
    }
}