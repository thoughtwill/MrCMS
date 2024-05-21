using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MrCMS.Data;
using MrCMS.Entities.Documents.Media;
using MrCMS.Mapping;
using MrCMS.Models;
using MrCMS.Services;
using MrCMS.Web.Admin.Models;
using NHibernate.Linq;

namespace MrCMS.Web.Admin.Services
{
    public class MediaCategoryAdminService : IMediaCategoryAdminService
    {
        private readonly IRepository<MediaCategory> _mediaCategoryRepository;
        private readonly IGetMediaCategoriesByParent _getMediaCategoriesByParent;
        private readonly IUrlValidationService _urlValidationService;
        private readonly ISessionAwareMapper _mapper;

        public MediaCategoryAdminService(IRepository<MediaCategory> mediaCategoryRepository,
            IGetMediaCategoriesByParent getMediaCategoriesByParent,
            IUrlValidationService urlValidationService,
            ISessionAwareMapper mapper)
        {
            _mediaCategoryRepository = mediaCategoryRepository;
            _getMediaCategoriesByParent = getMediaCategoriesByParent;
            _urlValidationService = urlValidationService;
            _mapper = mapper;
        }

        public AddMediaCategoryModel GetNewCategoryModel(int? id)
        {
            return new AddMediaCategoryModel
            {
                ParentId = id
            };
        }

        public async Task<MediaCategory> GetCategory(int? id)
        {
            return id.HasValue ? await _mediaCategoryRepository.Get(id.Value) : null;
        }

        public async Task<IMediaCategoryAdminService.CanAddCategoryResult> CanAdd(AddMediaCategoryModel model)
        {
            var mediaCategory = _mapper.Map<MediaCategory>(model);
            if (await _mediaCategoryRepository.Query()
                    .AnyAsync(x => x.Path == mediaCategory.Path))
                return new IMediaCategoryAdminService.CanAddCategoryResult
                    { Success = false, ErrorMessage = "Category already exists at this location" };
            return new IMediaCategoryAdminService.CanAddCategoryResult { Success = true };
        }

        public async Task<MediaCategory> Add(AddMediaCategoryModel model)
        {
            var mediaCategory = _mapper.Map<MediaCategory>(model);
            await _mediaCategoryRepository.Add(mediaCategory);
            return mediaCategory;
        }

        public async Task<MediaCategory> Update(UpdateMediaCategoryModel model)
        {
            var category = await GetCategory(model.Id);
            _mapper.Map(model, category);
            await _mediaCategoryRepository.Update(category);
            return category;
        }

        public async Task<MediaCategory> Delete(int id)
        {
            var mediaCategory = await _mediaCategoryRepository.Get(id);
            await _mediaCategoryRepository.Delete(mediaCategory);
            return mediaCategory;
        }

        public async Task<List<SortItem>> GetSortItems(int id)
        {
            return
                (await _getMediaCategoriesByParent.GetMediaCategories(await GetCategory(id)))
                .Select(
                    arg => new SortItem { Order = arg.DisplayOrder, Id = arg.Id, Name = arg.Name })
                .OrderBy(x => x.Order)
                .ToList();
        }

        public async Task SetOrders(List<SortItem> items)
        {
            await _mediaCategoryRepository.TransactAsync(async repository =>
            {
                foreach (var item in items)
                {
                    var mediaFile = await repository.Get(item.Id);
                    mediaFile.DisplayOrder = item.Order;
                    await repository.Update(mediaFile);
                }
            });
        }

        public async Task<bool> UrlIsValidForMediaCategory(int siteId, string urlSegment, int? id)
        {
            return await _urlValidationService.UrlIsValidForMediaCategory(siteId, urlSegment, id);
        }

        public async Task<UpdateMediaCategoryModel> GetEditModel(int id)
        {
            return _mapper.Map<UpdateMediaCategoryModel>(await GetCategory(id));
        }

        public async Task<MediaCategory> Get(int id)
        {
            return await _mediaCategoryRepository.Get(id);
        }
    }
}