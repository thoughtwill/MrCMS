using System;
using MrCMS.AI.Settings;

namespace MrCMS.AI.Services.Core;

public class AiProviderFactory : IAiProviderFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly AiSettings _settings;

    public AiProviderFactory(IServiceProvider serviceProvider, AiSettings settings)
    {
        _serviceProvider = serviceProvider;
        _settings = settings;
    }
    
    public IAiProvider GetProvider()
    {
        var providerType = Type.GetType(_settings.AiProvider);

        if (providerType == null || !typeof(IAiProvider).IsAssignableFrom(providerType))
        {
            throw new InvalidOperationException($"Invalid or unsupported AI provider type: {_settings.AiProvider}");
        }

        if (_serviceProvider.GetService(providerType) is not IAiProvider provider)
        {
            throw new InvalidOperationException($"Failed to resolve AI provider: {_settings.AiProvider}");
        }

        return provider;
    }
}