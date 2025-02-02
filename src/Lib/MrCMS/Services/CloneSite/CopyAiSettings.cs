using System.Threading.Tasks;
using MrCMS.AI.Settings;
using MrCMS.Entities.Multisite;
using MrCMS.Settings;

namespace MrCMS.Services.CloneSite
{
    [CloneSitePart(-100)]
    public class CopyAiSettings : ICloneSiteParts
    {
        private readonly IAiConfigurationProviderFactory _factory;

        public CopyAiSettings(IAiConfigurationProviderFactory factory)
        {
            _factory = factory;
        }

        public async Task Clone(Site @from, Site to, SiteCloneContext siteCloneContext)
        {
            var fromProvider = _factory.GetForSite(@from);
            var toProvider = _factory.GetForSite(to);
            var settingsBases = fromProvider.GetAllSettings();
            foreach (var @base in settingsBases)
            {
                await toProvider.SaveSettings(@base);
            }
        }
    }
}