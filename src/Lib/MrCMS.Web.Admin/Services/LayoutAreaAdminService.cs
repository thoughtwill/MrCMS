using System.Collections.Generic;
using System.Threading.Tasks;
using MrCMS.Data;
using MrCMS.Entities.Documents.Layout;
using MrCMS.Entities.Widget;
using MrCMS.Helpers;
using MrCMS.Mapping;
using MrCMS.Models;
using MrCMS.Web.Admin.Models;
using MrCMS.Website;
using MrCMS.Website.Caching;

namespace MrCMS.Web.Admin.Services
{
    public class LayoutAreaAdminService : ILayoutAreaAdminService
    {
        public const string LayoutAreasKey = "get-widgets-for-layout-{0}";
        private readonly IRepository<LayoutArea> _layoutAreaRepository;
        private readonly IRepository<Widget> _widgetRepository;
        private readonly ISessionAwareMapper _mapper;
        private readonly ICacheManager _cacheManager;
        private readonly IWidgetLoader _widgetLoader;

        public LayoutAreaAdminService(IRepository<LayoutArea> layoutAreaRepository,
            IRepository<Widget> widgetRepository,
            ISessionAwareMapper mapper, ICacheManager cacheManager, IWidgetLoader widgetLoader)
        {
            _layoutAreaRepository = layoutAreaRepository;
            _widgetRepository = widgetRepository;
            _mapper = mapper;
            _cacheManager = cacheManager;
            _widgetLoader = widgetLoader;
        }

        public AddLayoutAreaModel GetAddModel(int id)
        {
            return new AddLayoutAreaModel {LayoutId = id};
        }

        public async Task Add(AddLayoutAreaModel model)
        {
            var layoutArea = _mapper.Map<LayoutArea>(model);
            EnsureLayoutAreaIsSet(layoutArea);
            await _layoutAreaRepository.Add(layoutArea);
            _cacheManager.Clear();
        }

        public async Task<UpdateLayoutAreaModel> GetEditModel(int id)
        {
            return _mapper.Map<UpdateLayoutAreaModel>(await GetArea(id));
        }

        public async Task<Layout> GetLayout(int id)
        {
            var area = await GetArea(id);
            return area?.Layout.Unproxy();
        }

        public async Task<IList<Widget>> GetWidgets(int id)
        {
            return await _widgetLoader.GetWidgets(await GetArea(id));
        }

        private static void EnsureLayoutAreaIsSet(LayoutArea layoutArea)
        {
            if (layoutArea.Layout != null && layoutArea.Layout.LayoutAreas.Contains(layoutArea) == false)
                layoutArea.Layout.LayoutAreas.Add(layoutArea);
        }

        public async Task<LayoutArea> Update(UpdateLayoutAreaModel model)
        {
            var layoutArea = await GetArea(model.Id);
            _mapper.Map(model, layoutArea);

            EnsureLayoutAreaIsSet(layoutArea);
            await _layoutAreaRepository.Update(layoutArea);
            _cacheManager.Clear();
            return layoutArea;
        }

        public async Task<LayoutArea> GetArea(int layoutAreaId)
        {
            return await _layoutAreaRepository.Get(layoutAreaId);
        }

        public async Task<LayoutArea> DeleteArea(int id)
        {
            var area = await GetArea(id);
            _cacheManager.Clear();

            if (area.Layout?.LayoutAreas.Contains(area) == true)
                area.Layout.LayoutAreas.Remove(area);

            await _layoutAreaRepository.Delete(area);
            return area;
        }

        public async Task SetWidgetOrders(WidgetSortModel widgetSortModel)
        {
            await _widgetRepository.TransactAsync(async repository =>
            {
                foreach (var model in widgetSortModel.Widgets)
                {
                    
                    var widget = await repository.Get(model.Id);
                    widget.DisplayOrder = model.Order;
                    await repository.Update(widget);
                };
            });
        }

        public async Task<WidgetSortModel> GetSortModel(LayoutArea area)
        {
            return new(await GetWidgets(area.Id), area);
        }

    }
}