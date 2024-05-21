using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MrCMS.Entities.Documents;
using MrCMS.Entities.Documents.Web;
using MrCMS.Helpers;

namespace MrCMS.Services
{
    public class ValidWebpageChildrenService : IValidWebpageChildrenService
    {
        private readonly IExistAnyWebpageService _existAnyWebpageService;
        private readonly IWebpageMetadataService _webpageMetadataService;

        public ValidWebpageChildrenService(IExistAnyWebpageService existAnyWebpageService,
            IWebpageMetadataService webpageMetadataService)
        {
            _existAnyWebpageService = existAnyWebpageService;
            _webpageMetadataService = webpageMetadataService;
        }

        public async Task<IReadOnlyCollection<WebpageMetadata>> GetValidWebpageTypes(Webpage webpage,
            Func<WebpageMetadata, Task<bool>> predicate)
        {
            var webpageTypeDefinitions = new HashSet<WebpageMetadata>();
            var webpageMetadata = _webpageMetadataService.WebpageMetadata.ToHashSet();
            if (webpage == null)
                webpageTypeDefinitions = new HashSet<WebpageMetadata>(
                    webpageMetadata.Where(definition => !definition.RequiresParent));
            else
            {
                var webpageTypeDefinition =
                    webpageMetadata.FirstOrDefault(
                        definition => definition.TypeName == webpage.Unproxy().GetType().Name);

                var metadataList =
                    webpageTypeDefinition.ChildrenList.Select(_webpageMetadataService.GetMetadata);
                switch (webpageTypeDefinition.ChildrenListType)
                {
                    case ChildrenListType.BlackList:
                        var webpageMetadataList =
                            webpageMetadata.Except(metadataList).Where(def => !def.AutoBlacklist);
                        webpageMetadataList.ForEach(item => webpageTypeDefinitions.Add(item));
                        break;
                    case ChildrenListType.WhiteList:
                        metadataList.ForEach(metadata => webpageTypeDefinitions.Add(metadata));
                        break;
                }
            }

            webpageTypeDefinitions.RemoveWhere(
                definition => typeof(IUniquePage).IsAssignableFrom(definition.Type) &&
                              _existAnyWebpageService.ExistAny(definition.Type));
            if (predicate != null)
            {
                foreach (var metadata in webpageTypeDefinitions.ToList()) // copy
                {
                    if (!await predicate(metadata))
                    {
                        webpageTypeDefinitions.Remove(metadata);
                    }
                }
            }

            return webpageTypeDefinitions;
        }

        public async Task<bool> AnyValidWebpageTypes(Webpage webpage, Func<WebpageMetadata, Task<bool>> predicate)
        {
            return (await GetValidWebpageTypes(webpage, predicate)).Any();
        }
    }
}