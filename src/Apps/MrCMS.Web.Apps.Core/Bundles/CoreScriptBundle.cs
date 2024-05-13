using System.Collections.Generic;
using System.Threading.Tasks;
using MrCMS.Website.Optimization;

namespace MrCMS.Web.Apps.Core.Bundles
{
    public class CoreScriptBundle : IUIScriptBundle
    {
        public int Priority => int.MaxValue;
        public Task<bool> ShouldShow(string theme) => Task.FromResult(string.IsNullOrWhiteSpace(theme));

        public string Url => "/Apps/Core/assets/core.js";

        public IEnumerable<string> VendorFiles
        {
            get { yield break; }
        }
    }
}