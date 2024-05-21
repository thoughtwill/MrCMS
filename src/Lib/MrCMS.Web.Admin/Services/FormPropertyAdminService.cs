using Microsoft.AspNetCore.Mvc.Rendering;
using MrCMS.Entities.Documents.Web.FormProperties;
using MrCMS.Helpers;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MrCMS.Mapping;
using MrCMS.Web.Admin.Models;

namespace MrCMS.Web.Admin.Services
{
    public class FormPropertyAdminService : IFormPropertyAdminService
    {
        private readonly ISession _session;
        private readonly ISessionAwareMapper _mapper;

        private static readonly HashSet<Type> FormPropertyTypes =
            TypeHelper.GetAllConcreteTypesAssignableFrom<FormProperty>();

        private static readonly List<Type> OrderedTypes = new List<Type>
        {
            typeof(TextBox),
            typeof(Email),
            typeof(DropDownList),
            typeof(TextArea),
            typeof(CheckboxList),
            typeof(RadioButtonList),
            typeof(FileUpload)
        };

        public FormPropertyAdminService(ISession session, ISessionAwareMapper mapper)
        {
            _session = session;
            _mapper = mapper;
        }

        public async Task Add(AddFormPropertyModel model)
        {
            var type = TypeHelper.GetTypeByName(model.PropertyType);
            var property = Activator.CreateInstance(type) as FormProperty;
            if (property == null)
            {
                return;
            }

            _mapper.Map(model, property);

            property.Form?.FormProperties.Add(property);

            await _session.TransactAsync(async session =>
            {
                if (property.Form.FormProperties != null)
                {
                    property.DisplayOrder = property.Form.FormProperties.Count;
                }

                await session.SaveAsync(property);
            });
        }

        public async Task Update(UpdateFormPropertyModel model)
        {
            var property = await GetFormProperty(model.Id);
            _mapper.Map(model, property);
            await _session.TransactAsync(session => session.UpdateAsync(property));
        }

        public async Task Delete(int id)
        {
            var property = await GetFormProperty(id);
            property.Form.FormProperties.Remove(property);
            await _session.TransactAsync(session => session.DeleteAsync(property));
        }

        public List<SelectListItem> GetPropertyTypeOptions()
        {
            // but we still want to load all in case any custom types have been added
            return FormPropertyTypes
                .OrderBy(type => OrderedTypes.Contains(type))
                .ThenBy(type => OrderedTypes.IndexOf(type))
                .ThenBy(type => type.Name)
                .BuildSelectItemList(type => type.Name.BreakUpString(),
                    type => type.FullName,
                    emptyItemText: null);
        }

        public async Task<UpdateFormPropertyModel> GetUpdateModel(int id)
        {
            var formProperty = await GetFormProperty(id);
            return _mapper.Map<UpdateFormPropertyModel>(formProperty);
        }

        private async Task<FormProperty> GetFormProperty(int id)
        {
            return await _session.GetAsync<FormProperty>(id);
        }
    }
}