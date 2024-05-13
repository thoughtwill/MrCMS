using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MrCMS.Entities;
using MrCMS.Helpers;
using MrCMS.Services.Resources;
using MrCMS.Shortcodes;
using MrCMS.Web.Admin.Models;
using NHibernate;

namespace MrCMS.Web.Admin.Services
{
    public class InPageAdminService : IInPageAdminService
    {
        private readonly ISession _session;
        private readonly IStringResourceProvider _stringResourceProvider;
        private readonly IShortcodeParser _shortcodeParser;
        private readonly ILogger<InPageAdminService> _logger;

        public InPageAdminService(ISession session, IStringResourceProvider stringResourceProvider,
            IShortcodeParser shortcodeParser, ILogger<InPageAdminService> logger)
        {
            _session = session;
            _stringResourceProvider = stringResourceProvider;
            _shortcodeParser = shortcodeParser;
            _logger = logger;
        }

        public async Task<SaveResult> SaveContent(UpdatePropertyData updatePropertyData)
        {
            HashSet<Type> types = TypeHelper.GetAllConcreteTypesAssignableFrom<SystemEntity>();
            Type entityType = types.FirstOrDefault(t => t.Name == updatePropertyData.EntityType);
            if (entityType == null)
                return new SaveResult(false, await _stringResourceProvider.GetValue("Admin Inline Editing Save Entity Not Found", configureOptions => configureOptions.SetDefaultValue("Could not find entity type '{entityType}'").AddReplacement("entityType",updatePropertyData.EntityType)));
            object entity = _session.Get(entityType, updatePropertyData.Id);
            if (entity == null)
                return new SaveResult(false,await _stringResourceProvider.GetValue("Admin InlineEditing Save Not Found", configureOptions => configureOptions.SetDefaultValue("Could not find entity of type '{entityType}' with id {id}").AddReplacement("entityType",updatePropertyData.EntityType).AddReplacement("id",updatePropertyData.Id.ToString())));
            PropertyInfo propertyInfo =
                entityType.GetProperties().FirstOrDefault(info => info.Name == updatePropertyData.EntityProperty);
            if (propertyInfo == null)
                return new SaveResult(false,await _stringResourceProvider.GetValue("Admin InlineEditing Save NotFound", configureOptions => configureOptions.SetDefaultValue("Could not find entity of type '{entityType}' with id {id}").AddReplacement("entityType",updatePropertyData.EntityType).AddReplacement("id",updatePropertyData.Id.ToString())));
            if (propertyInfo.GetCustomAttributes(true).FirstOrDefault(x => x.GetType() == typeof(StringLengthAttribute))
                    is StringLengthAttribute stringLengthAttribute &&
                updatePropertyData.Content.Length > stringLengthAttribute.MaximumLength)
            {
                return new SaveResult
                {
                    success = false,
                    message = await _stringResourceProvider.GetValue("Admin InlineEditing Save MaxLength", configureOptions => configureOptions.SetDefaultValue("Could not save property. The maximum length for this field is {maxLength} characters.").AddReplacement("maxLength",stringLengthAttribute.MaximumLength.ToString()))
                };
            }

            if (string.IsNullOrWhiteSpace(updatePropertyData.Content) &&
                propertyInfo.GetCustomAttributes(typeof(RequiredAttribute), false).Any())
            {
                return new SaveResult(false,await _stringResourceProvider.GetValue("Admin InlineEditing Save Required", configureOptions => configureOptions.SetDefaultValue("Could not edit '{entityProperty}' as it is required").AddReplacement("entityProperty",updatePropertyData.EntityProperty)));
            }

            try
            {
                propertyInfo.SetValue(entity, updatePropertyData.Content, null);
                await _session.TransactAsync(session => session.SaveOrUpdateAsync(entity));
            }
            catch (Exception exception)
            {
                _logger.Log(LogLevel.Error, exception, exception.Message);

                return new SaveResult(false,
                    await _stringResourceProvider.GetValue("Admin InlineEditing Save Error", configureOptions => configureOptions.SetDefaultValue("Could not save to database '{entityProperty}' due to unknown error. Please check log.").AddReplacement("entityProperty",updatePropertyData.EntityProperty)));
            }

            return new SaveResult();
        }


        public async Task<ContentInfo> GetContent(GetPropertyData getPropertyData)
        {
            HashSet<Type> types = TypeHelper.GetAllConcreteTypesAssignableFrom<SystemEntity>();
            Type entityType = types.FirstOrDefault(t => t.Name == getPropertyData.Type);
            if (entityType == null)
                return new ContentInfo();
            object entity = await _session.GetAsync(entityType, getPropertyData.Id);
            if (entity == null)
                return new ContentInfo();
            PropertyInfo propertyInfo =
                entityType.GetProperties().FirstOrDefault(info => info.Name == getPropertyData.Property);
            if (propertyInfo == null)
                return new ContentInfo();
            return new ContentInfo
            {
                Content = Convert.ToString(propertyInfo.GetValue(entity, null)),
                Entity = entity
            };
        }
    }
}