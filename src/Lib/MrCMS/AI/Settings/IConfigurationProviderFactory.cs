using MrCMS.Entities.Multisite;

namespace MrCMS.AI.Settings
{
    public interface IAiConfigurationProviderFactory
    {
        IAiConfigurationProvider GetForSite(Site site);
    }
}