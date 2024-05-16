using Microsoft.AspNetCore.Mvc.ViewFeatures;
using MrCMS.Entities.Documents;
using MrCMS.Entities.Documents.Web;
using MrCMS.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MrCMS.Web.Admin.Models;

namespace MrCMS.Web.Admin.Services
{
    public class WebpageBaseViewDataService : IWebpageBaseViewDataService
    {
        private readonly IGetValidPageTemplatesToAdd _getValidPageTemplatesToAdd;
        private readonly IWebpageMetadataService _webpageMetadataService;
        private readonly IValidWebpageChildrenService _validWebpageChildrenService;

        public WebpageBaseViewDataService(IValidWebpageChildrenService validWebpageChildrenService,
            IGetValidPageTemplatesToAdd getValidPageTemplatesToAdd,
            IWebpageMetadataService webpageMetadataService)
        {
            _validWebpageChildrenService = validWebpageChildrenService;
            _getValidPageTemplatesToAdd = getValidPageTemplatesToAdd;
            _webpageMetadataService = webpageMetadataService;
        }

        public async Task SetAddPageViewData(ViewDataDictionary viewData, Webpage parent)
        {
            viewData["parent"] = parent;

            // TODO - replace this with explicit ACL if we decide to re-add it to WebpageController
            var webpageWebpageTypes = await _validWebpageChildrenService
                .GetValidWebpageTypes(parent, _ => Task.FromResult(true));
            
            var validWebpageWebpageTypes =
                webpageWebpageTypes.OrderBy(metadata => metadata.DisplayOrder).ToList();

            var templates = _getValidPageTemplatesToAdd.Get(validWebpageWebpageTypes);

            var webpageTypeToAdds = new List<WebpageTypeToAdd>();

            foreach (WebpageMetadata type in validWebpageWebpageTypes)
            {
                if (templates.Any(template => template.PageType == type.Type.FullName))
                {
                    webpageTypeToAdds.Add(new WebpageTypeToAdd
                    {
                        Type = type,
                        DisplayName = $"Default {type.Name}"
                    });
                    WebpageMetadata typeCopy = type;
                    webpageTypeToAdds.AddRange(
                        templates.Where(template => template.PageType == typeCopy.Type.FullName)
                            .Select(pageTemplate => new WebpageTypeToAdd
                            {
                                Type = type,
                                DisplayName = pageTemplate.Name,
                                TemplateId = pageTemplate.Id
                            }));
                }
                else
                {
                    webpageTypeToAdds.Add(new WebpageTypeToAdd {Type = type, DisplayName = type.Name});
                }
            }

            viewData["WebpageTypes"] = webpageTypeToAdds;
        }

        public Task SetEditPageViewData(ViewDataDictionary viewData, Webpage page)
        {
            WebpageMetadata webpageMetadata = _webpageMetadataService.GetMetadata(page);
            viewData["EditView"] = webpageMetadata.EditPartialView;
            return Task.CompletedTask;
        }
    }
}