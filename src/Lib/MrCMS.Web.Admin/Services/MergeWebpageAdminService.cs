using Microsoft.AspNetCore.Mvc.Rendering;
using MrCMS.Data;
using MrCMS.Entities.Documents;
using MrCMS.Entities.Documents.Web;
using MrCMS.Helpers;
using MrCMS.Models;
using MrCMS.Services;
using MrCMS.Services.Resources;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MrCMS.Web.Admin.Models;
using NHibernate.Linq;


namespace MrCMS.Web.Admin.Services
{
    public class MergeWebpageAdminService : IMergeWebpageAdminService
    {
        private readonly IRepository<Webpage> _webpageRepository;
        private readonly IStringResourceProvider _resourceProvider;
        private readonly IWebpageUrlService _webpageUrlService;
        private readonly ICreateMergeBatch _createMergeBatch;
        private readonly IWebpageMetadataService _webpageMetadataService;

        public MergeWebpageAdminService(IRepository<Webpage> webpageRepository,
            IStringResourceProvider resourceProvider, IWebpageUrlService webpageUrlService,
            ICreateMergeBatch createMergeBatch,
            IWebpageMetadataService webpageMetadataService)
        {
            _webpageRepository = webpageRepository;
            _resourceProvider = resourceProvider;
            _webpageUrlService = webpageUrlService;
            _createMergeBatch = createMergeBatch;
            _webpageMetadataService = webpageMetadataService;
        }

        public async Task<List<SelectListItem>> GetValidParents(Webpage webpage)
        {
            var children = await _webpageRepository.Query().Where(x => x.Parent.Id == webpage.Id).ToListAsync();
            // get the matching pages for all types
            var potentialParents = new HashSet<IReadOnlyList<Webpage>>();
            foreach (var child in children)
            {
                potentialParents.Add(await GetValidParentWebpages(child));
            }

            var validForAll = potentialParents.SelectMany(x => x).Distinct()
                .Where(x => x != webpage && potentialParents.All(y => y.Contains(x))).ToHashSet();

            var items = validForAll.BuildSelectItemList(
                page => $"{page.Name} ({_webpageMetadataService.GetMetadata(page).Name})",
                page => page.Id.ToString(),
                webpage1 => webpage.Parent != null && webpage.Parent.Id == webpage1.Id, emptyItem: null);

            return items;
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

        public async Task<MergeWebpageResult> Validate(MergeWebpageModel moveWebpageModel)
        {
            var webpage = await GetWebpage(moveWebpageModel);
            var parent = await GetParent(moveWebpageModel);

            var validParentWebpages = await GetValidParentWebpages(webpage);
            var valid = webpage != null && parent != null && validParentWebpages.Contains(parent);

            return new MergeWebpageResult
            {
                Success = valid,
                Message = valid
                    ? string.Empty
                    : await _resourceProvider.GetValue("Sorry, but you can't select that as a parent for this page.")
            };
        }

        private async Task<Webpage> GetParent(MergeWebpageModel moveWebpageModel)
        {
            return await _webpageRepository.Get(moveWebpageModel.MergeIntoId);
        }

        private async Task<Webpage> GetWebpage(MergeWebpageModel moveWebpageModel)
        {
            return await _webpageRepository.Get(moveWebpageModel.Id);
        }

        public async Task<MergeWebpageConfirmationModel> GetConfirmationModel(MergeWebpageModel model)
        {
            var webpage = await GetWebpage(model);
            var parent = await GetParent(model);

            return new MergeWebpageConfirmationModel
            {
                Webpage = webpage,
                MergedInto = parent,
                ChangedPages = await GetChangedPages(model, webpage, parent)
            };
        }

        private async Task<List<MergeWebpageChangedPageModel>> GetChangedPages(MergeWebpageModel model, Webpage webpage,
            Webpage parent)
        {
            var webpageHierarchy = await GetWebpageHierarchy(webpage);

            var parentActivePages = (parent.ActivePages.Reverse()).ToList();
            List<MergeWebpageChangedPageModel> models = new List<MergeWebpageChangedPageModel>();
            foreach (var page in webpageHierarchy)
            {
                var activePages = page.ActivePages.ToList();
                var indexOf = activePages.IndexOf(webpage);
                var childActivePages = activePages.Take(indexOf).ToList();
                activePages.Reverse();
                var immediateParent = childActivePages.ElementAtOrDefault(1);
                childActivePages.Reverse();
                var newUrl = await GetNewUrl(model, parent, page, immediateParent,
                    models.FirstOrDefault(x => x.Id == immediateParent?.Id));
                models.Add(new MergeWebpageChangedPageModel
                {
                    Id = page.Id,
                    ParentId = immediateParent?.Id ?? model.Id,
                    OldUrl = page.UrlSegment,
                    NewUrl = newUrl,
                    OldHierarchy = GetHierarchy(activePages),
                    NewHierarchy = GetHierarchy(parentActivePages.Concat(childActivePages))
                });
            }

            return models;
        }

        private async Task<string> GetNewUrl(MergeWebpageModel model, Webpage parent, Webpage page,
            Webpage immediateParent,
            MergeWebpageChangedPageModel parentModel)
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
                    WebpageId = page.Id,
                    ParentId = parent?.Id
                });
            }

            return await _webpageUrlService.Suggest(page.Site.Id, new SuggestParams
            {
                WebpageType = page.WebpageType,
                PageName = $"{parentModel.NewUrl}/{page.Name}",
                Template = page.PageTemplate?.Id,
                UseHierarchy = false,
                WebpageId = page.Id,
                ParentId = immediateParent.Id
            });
        }

        private string GetHierarchy(IEnumerable<Webpage> webpages)
        {
            return string.Join(" > ", webpages.Select(x => x.Name));
        }

        private async Task<IReadOnlyList<Webpage>> GetWebpageHierarchy(Webpage webpage)
        {
            var descendants = await _webpageRepository.Query().Where(x => x.Parent.Id == webpage.Id)
                .OrderBy(x => x.DisplayOrder)
                .ToListAsync();
            var result = new List<Webpage>();
            foreach (var descendant in descendants)
            {
                result.Add(descendant);
                result.AddRange(await GetWebpageHierarchy(descendant));
            }

            return result;
        }


        public async Task<MergeWebpageResult> Confirm(MergeWebpageModel model)
        {
            var confirmationModel = await GetConfirmationModel(model);

            var success = await _createMergeBatch.CreateBatch(confirmationModel);

            return new MergeWebpageResult
            {
                Success = success,
                Message = await _resourceProvider.GetValue(success
                    ? "The page has been moved successfully"
                    : "There was an issue creating the batch to complete the merge")
            };
        }

        public MergeWebpageModel GetModel(Webpage webpage)
        {
            return new MergeWebpageModel
            {
                Id = webpage.Id
            };
        }
    }
}