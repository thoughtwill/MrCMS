using System;
using MrCMS.AI.Settings;

namespace MrCMS.AI.Services.Core;

public class AiTextProviderFactory : IAiTextProviderFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly AiSettings _settings;

    public AiTextProviderFactory(IServiceProvider serviceProvider, AiSettings settings)
    {
        _serviceProvider = serviceProvider;
        _settings = settings;
    }
    
    public IAiTextProvider GetProvider()
    {
        var providerType = Type.GetType(_settings.AiTextProvider);

        if (providerType == null || !typeof(IAiTextProvider).IsAssignableFrom(providerType))
        {
            throw new InvalidOperationException($"Invalid or unsupported AI provider type: {_settings.AiTextProvider}");
        }

        if (_serviceProvider.GetService(providerType) is not IAiTextProvider provider)
        {
            throw new InvalidOperationException($"Failed to resolve AI provider: {_settings.AiTextProvider}");
        }

        return provider;
    }
}