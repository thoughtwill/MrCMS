using System.Collections.Generic;
using System.Threading.Tasks;
using MrCMS.Website.Optimization;

namespace MrCMS.Web.Apps.Core.Bundles
{
    public class CoreAdminScriptBundle : IAdminScriptBundle
    {
        public int Priority => int.MaxValue;

        public Task<bool> ShouldShow(string theme)
        {
            return Task.FromResult(true);
        }

        public string Url => "/Apps/Core/assets/core-admin.js";

        public IEnumerable<string> VendorFiles
        {
            get
            {
                yield break;
            }
        }
    }
}