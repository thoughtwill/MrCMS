using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using MrCMS.Data;
using MrCMS.Entities.Documents;
using MrCMS.Entities.Documents.Web;
using MrCMS.Helpers;
using MrCMS.Models;
using MrCMS.Services;
using MrCMS.Services.Resources;
using MrCMS.Web.Admin.Models;
using NHibernate.Linq;

namespace MrCMS.Web.Admin.Services
{
    public class MoveWebpageAdminService : IMoveWebpageAdminService
    {
        private readonly IRepository<Webpage> _webpageRepository;
        private readonly IStringResourceProvider _resourceProvider;
        private readonly IWebpageUrlService _webpageUrlService;
        private readonly ICreateUpdateUrlBatch _createUpdateUrlBatch;
        private readonly IWebpageMetadataService _webpageMetadataService;

        public MoveWebpageAdminService(IRepository<Webpage> webpageRepository, IStringResourceProvider resourceProvider,
            IWebpageUrlService webpageUrlService, ICreateUpdateUrlBatch createUpdateUrlBatch,
            IWebpageMetadataService webpageMetadataService)
        {
            _webpageRepository = webpageRepository;
            _resourceProvider = resourceProvider;
            _webpageUrlService = webpageUrlService;
            _createUpdateUrlBatch = createUpdateUrlBatch;
            _webpageMetadataService = webpageMetadataService;
        }

        public async Task<List<SelectListItem>> GetValidParents(Webpage webpage)
        {
            var webpages = await GetValidParentWebpages(webpage);
            List<SelectListItem> result = webpages
                .BuildSelectItemList(
                    page => $"{page.Name} ({_webpageMetadataService.GetMetadata(page).Name})",
                    page => page.Id.ToString(),
                    webpage1 => webpage.Parent != null && webpage.Parent.Id == webpage1.Id, emptyItem: null);

            if (IsRootAllowed(webpage))
            {
                result.Insert(0, SelectListItemHelper.EmptyItem("Root"));
            }

            return result;
        }

        private async Task<IReadOnlyList<Webpage>> GetValidParentWebpages(Webpage webpage)
        {
            List<WebpageMetadata> validParentTypes = _webpageMetadataService.GetValidParentTypes(webpage);

            List<string> validParentTypeNames =
                validParentTypes.Select(documentMetadata => documentMetadata.Type.FullName).ToList();
            IList<Webpage> potentialParents =
                await _webpageRepository.Query()
                    .Where(page => validParentTypeNames.Contains(page.WebpageTypeName))
                    .ToListAsync();

            var webpages = potentialParents.Distinct()
                .Where(page => !page.ActivePages.Contains(webpage))
                .OrderBy(x => x.Name);
            return webpages.ToList();
        }

        private async Task<bool> SetParent(Webpage webpage, Webpage parent)
        {
            if (webpage == null)
            {
                return false;
            }

            webpage.Parent = parent;

            await _webpageRepository.Update(webpage);
            return true;
        }

        public async Task<MoveWebpageResult> Validate(MoveWebpageModel moveWebpageModel)
        {
            var webpage = await GetWebpage(moveWebpageModel);
            var parent = await GetParent(moveWebpageModel);

            if (parent == webpage.Parent.Unproxy())
            {
                return new MoveWebpageResult
                {
                    Success = false,
                    Message = await _resourceProvider.GetValue("The webpage already has this parent")
                };
            }

            var validParentWebpages = await GetValidParentWebpages(webpage);
            var valid = parent == null ? IsRootAllowed(webpage) : validParentWebpages.Contains(parent);

            return new MoveWebpageResult
            {
                Success = valid,
                Message = valid
                    ? string.Empty
                    : await _resourceProvider.GetValue("Sorry, but you can't select that as a parent for this page.")
            };
        }

        private async Task<Webpage> GetParent(MoveWebpageModel moveWebpageModel)
        {
            return moveWebpageModel.ParentId.HasValue
                ? await _webpageRepository.Get(moveWebpageModel.ParentId.Value)
                : null;
        }

        private async Task<Webpage> GetWebpage(MoveWebpageModel moveWebpageModel)
        {
            return await _webpageRepository.Get(moveWebpageModel.Id);
        }

        private bool IsRootAllowed(Webpage webpage)
        {
            return !_webpageMetadataService.GetMetadata(webpage).RequiresParent;
        }

        public async Task<MoveWebpageConfirmationModel> GetConfirmationModel(MoveWebpageModel model)
        {
            var webpage = await GetWebpage(model);
            var parent = await GetParent(model);


            return new MoveWebpageConfirmationModel
            {
                Webpage = webpage,
                Parent = parent,
                ChangedPages = await GetChangedPages(model, webpage, parent)
            };
        }

        private async Task<List<MoveWebpageChangedPageModel>> GetChangedPages(MoveWebpageModel model, Webpage webpage,
            Webpage parent)
        {
            var webpageHierarchy = await GetWebpageHierarchy(webpage);

            var parentActivePages = (parent?.ActivePages.Reverse() ?? Enumerable.Empty<Webpage>()).ToList();
            List<MoveWebpageChangedPageModel> models = new List<MoveWebpageChangedPageModel>();
            foreach (var page in webpageHierarchy)
            {
                var activePages = page.ActivePages.ToList();
                var indexOf = activePages.IndexOf(webpage);
                var childActivePages = activePages.Take(indexOf + 1).ToList();
                activePages.Reverse();
                var immediateParent = childActivePages.ElementAtOrDefault(1);
                childActivePages.Reverse();
                var newUrl = await GetNewUrl(model, parent, page, immediateParent,
                    models.FirstOrDefault(x => x.Id == immediateParent?.Id));
                models.Add(new MoveWebpageChangedPageModel
                {
                    Id = page.Id,
                    ParentId = immediateParent?.Id,
                    OldUrl = page.UrlSegment,
                    NewUrl = newUrl,
                    OldHierarchy = GetHierarchy(activePages),
                    NewHierarchy = GetHierarchy(parentActivePages.Concat(childActivePages))
                });
            }

            return models;
        }

        private async Task<string> GetNewUrl(MoveWebpageModel model, Webpage parent, Webpage page,
            Webpage immediateParent,
            MoveWebpageChangedPageModel parentModel)
        {
            if (!model.UpdateUrls)
            {
                return page.UrlSegment;
            }

            if (immediateParent == null)
            {
                return await _webpageUrlService.Suggest(page.Site.Id, new SuggestParams
                {
                    WebpageType = page.WebpageType,
                    PageName = page.Name,
                    Template = page.PageTemplate?.Id,
                    UseHierarchy = true,
                    ParentId = parent?.Id,
                    WebpageId = page.Id
                });
            }

            return await _webpageUrlService.Suggest(page.Site.Id, new SuggestParams
            {
                WebpageType = page.WebpageType,
                PageName = $"{parentModel.NewUrl}/{page.Name}",
                Template = page.PageTemplate?.Id,
                UseHierarchy = false,
                ParentId = immediateParent.Id,
                WebpageId = page.Id
            });
        }

        private string GetHierarchy(IEnumerable<Webpage> webpages)
        {
            return string.Join(" > ", webpages.Select(x => x.Name));
        }

        private async Task<IReadOnlyList<Webpage>> GetWebpageHierarchy(Webpage webpage)
        {
            var result = new List<Webpage> { webpage };
            var descendants = await _webpageRepository.Query().Where(x => x.Parent.Id == webpage.Id)
                .OrderBy(x => x.DisplayOrder)
                .ToListAsync();
            foreach (var descendant in descendants)
            {
                result.AddRange(await GetWebpageHierarchy(descendant));
            }

            return result;
        }

        public async Task<MoveWebpageResult> Confirm(MoveWebpageModel model)
        {
            var confirmationModel = await GetConfirmationModel(model);

            var success = await SetParent(confirmationModel.Webpage, confirmationModel.Parent);
            if (!success)
            {
                return new MoveWebpageResult
                {
                    Success = false,
                    Message = await _resourceProvider.GetValue("There was an issue setting the parent to the new value")
                };
            }

            success = await _createUpdateUrlBatch.CreateBatch(confirmationModel);

            return new MoveWebpageResult
            {
                Success = success,
                Message = await _resourceProvider.GetValue(success
                    ? "The page has been moved successfully"
                    : "There was an issue creating the batch to update the page URLs")
            };
        }

        public MoveWebpageModel GetModel(Webpage webpage)
        {
            return new MoveWebpageModel
            {
                Id = webpage.Id
            };
        }
    }
}