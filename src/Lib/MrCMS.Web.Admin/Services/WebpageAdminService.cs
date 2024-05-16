using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MrCMS.Data;
using MrCMS.Entities.Documents.Web;
using MrCMS.Helpers;
using MrCMS.Mapping;
using MrCMS.Models;
using MrCMS.Services;
using MrCMS.Settings;
using MrCMS.Web.Admin.Models;
using MrCMS.Web.Admin.Infrastructure.ModelBinding;
using NHibernate.Linq;
using X.PagedList;

namespace MrCMS.Web.Admin.Services
{
    public class WebpageAdminService : IWebpageAdminService
    {
        private readonly IRepository<Webpage> _webpageRepository;
        private readonly ISessionAwareMapper _mapper;
        private readonly IGetWebpagesByParent _getWebpagesByParent;
        private readonly IWebpageMetadataService _webpageMetadataService;
        private readonly IOptions<SystemConfig> _config;

        public WebpageAdminService(IRepository<Webpage> webpageRepository,
            ISessionAwareMapper mapper,
            IGetWebpagesByParent getWebpagesByParent,
            IWebpageMetadataService webpageMetadataService, IOptions<SystemConfig> config)
        {
            _webpageRepository = webpageRepository;
            _mapper = mapper;
            _getWebpagesByParent = getWebpagesByParent;
            _webpageMetadataService = webpageMetadataService;
            _config = config;
        }


        public async Task<Webpage> GetWebpage(int? id)
        {
            return id.HasValue ? await _webpageRepository.Get(id.Value) : null;
        }

        public AddWebpageModel GetAddModel(int? id)
        {
            return new AddWebpageModel
            {
                ParentId = id
            };
        }

        private static readonly HashSet<Type> AddPropertiesTypes = TypeHelper.GetAllConcreteTypesAssignableFromGeneric(
            typeof(IAddPropertiesViewModel<>));

        public object GetAdditionalPropertyModel(string type)
        {
            var webpageType = TypeHelper.GetTypeByName(type);
            if (webpageType == null)
                return null;
            var additionalPropertyType = AddPropertiesTypes.FirstOrDefault(x =>
                typeof(IAddPropertiesViewModel<>).MakeGenericType(webpageType).IsAssignableFrom(x));
            if (additionalPropertyType == null)
                return null;

            return Activator.CreateInstance(additionalPropertyType);
        }


        public async Task<Webpage> Add(AddWebpageModel model, object additionalPropertyModel)
        {
            var type = TypeHelper.GetTypeByName(model.WebpageType);
            var instance = Activator.CreateInstance(type) as Webpage;
            var revealInNavigation = _webpageMetadataService.GetMetadata(instance).RevealInNavigation;
            _mapper.Map(model, instance);

            if (revealInNavigation)
                if (instance != null)
                    instance.RevealInNavigation = true;

            if (additionalPropertyModel != null)
                _mapper.Map(additionalPropertyModel, instance);

            await _webpageRepository.Add(instance);

            return instance;
        }

        public async Task<Webpage> Update(UpdateWebpageViewModel viewModel)
        {
            var webpage = await GetWebpage(viewModel.Id);

            _mapper.Map(viewModel, webpage);

            foreach (var model in viewModel.Models)
                _mapper.Map(model, webpage);

            await _webpageRepository.Update(webpage);

            return webpage;
        }

        public async Task<Webpage> Delete(int id)
        {
            var webpage = await GetWebpage(id);
            await _webpageRepository.Delete(webpage);
            return webpage;
        }

        public async Task<List<SortItem>> GetSortItems(int? id)
        {
            var parent = await GetWebpage(id);
            return (await _getWebpagesByParent.GetWebpages(parent))
                .Select(
                    arg => new SortItem {Order = arg.DisplayOrder, Id = arg.Id, Name = arg.Name})
                .OrderBy(x => x.Order)
                .ToList();
        }

        public async Task SetOrders(List<SortItem> items)
        {
            await _webpageRepository.TransactAsync(async repository =>
            {
                foreach (var item in items)
                {
                    var mediaFile = await repository.Get(item.Id);
                    mediaFile.DisplayOrder = item.Order;
                    await repository.Update(mediaFile);
                }
            });
        }

        public async Task PublishNow(int id)
        {
            var webpage = await GetWebpage(id);
            if (webpage == null)
                return;

            if (webpage.PublishOn == null)
            {
                webpage.Published = true;
                webpage.PublishOn = DateTime.UtcNow;
                await _webpageRepository.Update(webpage);
            }
        }

        public async Task Unpublish(int id)
        {
            var webpage = await GetWebpage(id);
            if (webpage == null)
                return;
            webpage.Published = false;
            webpage.PublishOn = null;
            await _webpageRepository.Update(webpage);
        }

        public string GetServerDate()
        {
            var now = DateTime.UtcNow;
            return now.Add(_config.Value.TimeZoneInfo.GetUtcOffset(now)).ToString(CultureInfo.InvariantCulture);
        }

        public async Task<IPagedList<Select2LookupResult>> Search(string term, int page)
        {
            return await _webpageRepository.Query()
                .Where(x => x.UrlSegment.Like($"%{term}%") || x.Name.Like($"%{term}%"))
                .OrderByDescending(x => x.CreatedOn)
                .Select(x => new Select2LookupResult
                {
                    id = x.Id,
                    text = x.Name
                })
                .PagedAsync(page);
        }
    }
}