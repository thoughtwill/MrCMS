using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MrCMS.Entities.Widget;
using MrCMS.Helpers;
using MrCMS.Mapping;
using MrCMS.Web.Admin.Models;
using MrCMS.Web.Admin.Infrastructure.ModelBinding;
using NHibernate;

namespace MrCMS.Web.Admin.Services
{
    public class WidgetAdminService : IWidgetAdminService
    {
        private readonly ISession _session;
        private readonly ISessionAwareMapper _mapper;

        public WidgetAdminService(ISession session, ISessionAwareMapper mapper)
        {
            _session = session;
            _mapper = mapper;
        }

        private static readonly HashSet<Type> AddPropertiesTypes = TypeHelper.GetAllConcreteTypesAssignableFromGeneric(
            typeof(IAddPropertiesViewModel<>));
        
        private static readonly HashSet<Type> UpdatePropertiesTypes = TypeHelper.GetAllConcreteTypesAssignableFromGeneric(
            typeof(IUpdatePropertiesViewModel<>));

        public object GetAdditionalPropertyModel(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
                return null;

            var webpageType = TypeHelper.GetTypeByName(type);
            if (webpageType == null)
                return null;
            var additionalPropertyType = AddPropertiesTypes.FirstOrDefault(x =>
                typeof(IAddPropertiesViewModel<>).MakeGenericType(webpageType).IsAssignableFrom(x));
            if (additionalPropertyType == null)
                return null;

            return Activator.CreateInstance(additionalPropertyType);
        }
        
        public object GetUpdateAdditionalPropertyModel(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
                return null;

            var widgetType = TypeHelper.GetTypeByName(type);
            if (widgetType == null)
                return null;
            var additionalPropertyType = UpdatePropertiesTypes.FirstOrDefault(x =>
                typeof(IUpdatePropertiesViewModel<>).MakeGenericType(widgetType).IsAssignableFrom(x));
            if (additionalPropertyType == null)
                return null;

            return Activator.CreateInstance(additionalPropertyType);
        }

        public async Task<Widget> AddWidget(AddWidgetModel model, object additionalPropertyModel)
        {
            var type = TypeHelper.GetTypeByName(model.WidgetType);
            var instance = Activator.CreateInstance(type) as Widget;
            _mapper.Map(model, instance);
            if (additionalPropertyModel != null)
                _mapper.Map(additionalPropertyModel, instance);

            instance.LayoutArea.AddWidget(instance);
            await _session.TransactAsync(session => session.SaveAsync(instance));

            return instance;
        }

        public async Task<UpdateWidgetModel> GetEditModel(int id)
        {
            return _mapper.Map<UpdateWidgetModel>(await GetWidget(id));
        }

        public async Task<Widget> GetWidget(int id)
        {
            return await _session.GetAsync<Widget>(id);
        }

        public async Task<object> GetAdditionalPropertyModel(int id)
        {
            var widget = await GetWidget(id);
            return GetAdditionalPropertyModel(widget?.WidgetType);
        }
        
        public async Task<object> GetUpdateAdditionalPropertyModel(int id)
        {
            var widget = await GetWidget(id);
            return GetUpdateAdditionalPropertyModel(widget?.WidgetType);
        }

        public async Task<Widget> UpdateWidget(UpdateWidgetModel model, object additionalPropertyModel)
        {
            var widget = await GetWidget(model.Id);
            _mapper.Map(model, widget);

            if (additionalPropertyModel != null)
                _mapper.Map(additionalPropertyModel, widget);

            await _session.TransactAsync(session => session.UpdateAsync(widget));

            return widget;
        }

        public async Task<Widget> DeleteWidget(int id)
        {
            var widget = await GetWidget(id);
            widget.LayoutArea.Widgets.Remove(widget);
            await _session.TransactAsync(session => session.DeleteAsync(widget));

            return widget;
        }
    }
}