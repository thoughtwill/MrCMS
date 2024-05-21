using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MrCMS.Entities.Messaging;
using MrCMS.Entities.Multisite;
using MrCMS.Helpers;
using Newtonsoft.Json;
using NHibernate;
using NHibernate.Linq;

namespace MrCMS.Messages
{
    public class MessageTemplateProvider : IMessageTemplateProvider
    {
        private static readonly Dictionary<Type, Type> DefaultTemplateProviders = new Dictionary<Type, Type>();

        private static readonly MethodInfo GetMessageTemplateMethod = typeof(MessageTemplateProvider)
            .GetMethodExt(nameof(GetMessageTemplate), typeof(Site));

        public static readonly MethodInfo GetNewMessageTemplateMethod = typeof(MessageTemplateProvider)
            .GetMethodExt(nameof(GetNewMessageTemplate));

        private readonly IServiceProvider _serviceProvider;
        private readonly ISession _session;

        static MessageTemplateProvider()
        {
            var templateTypes = TypeHelper.GetAllConcreteTypesAssignableFromGeneric(typeof(GetDefaultTemplate<>));
            foreach (var type in
                TypeHelper.GetAllConcreteTypesAssignableFrom<MessageTemplate>()
                    .Where(type => !type.ContainsGenericParameters))
            {
                var types = templateTypes.FindAll(x =>
                    typeof(GetDefaultTemplate<>).MakeGenericType(type).IsAssignableFrom(x));
                if (types.Any())
                {
                    DefaultTemplateProviders.Add(type, types.First());
                }
            }
        }

        public MessageTemplateProvider(ISession session, IServiceProvider serviceProvider)
        {
            _session = session;
            _serviceProvider = serviceProvider;
        }

        public async Task SaveTemplate(MessageTemplate messageTemplate)
        {
            await Save(messageTemplate, null);
        }

        public async Task SaveSiteOverride(MessageTemplate messageTemplate, Site site)
        {
            await Save(messageTemplate, site.Id);
        }

        public async Task DeleteSiteOverride(MessageTemplate messageTemplate, Site site)
        {
            var templateData = await GetExistingTemplateData(messageTemplate.GetType().FullName, site.Id);
            if (templateData != null)
                await _session.TransactAsync(session => session.DeleteAsync(templateData));
        }

        public async Task<List<MessageTemplate>> GetAllMessageTemplates(Site site)
        {
            var types = TypeHelper.GetAllConcreteTypesAssignableFrom<MessageTemplate>();

            var templates = new List<MessageTemplate>();
            foreach (var type in types)
            {
                var o = await GetMessageTemplateMethod.MakeGenericMethod(type)
                    .InvokeAsync(this, new object[] {site});
                if (o is MessageTemplate messageTemplate)
                    templates.Add(messageTemplate);
            }

            return templates;
        }

        public async Task<T> GetMessageTemplate<T>(Site site) where T : MessageTemplate, new()
        {
            var templateType = typeof(T);
            var fullName = templateType.FullName;
            var templateData = await GetExistingTemplateData(fullName, site.Id);
            if (templateData != null)
                return JsonConvert.DeserializeObject<T>(templateData.Data);

            templateData = await GetExistingTemplateData(fullName, null);
            if (templateData != null)
                return JsonConvert.DeserializeObject<T>(templateData.Data);

            var template = await GetNewMessageTemplate<T>();
            await SaveTemplate(template);
            return template;
        }

        public async Task<MessageTemplate> GetNewMessageTemplate(Type type)
        {
            return await GetNewMessageTemplateMethod.MakeGenericMethod(type).InvokeAsync(this, new object[0]) as
                MessageTemplate;
        }

        private async Task Save(MessageTemplate messageTemplate, int? siteId)
        {
            var type = messageTemplate.GetType().FullName;
            var existingData = await GetExistingTemplateData(type, siteId) ??
                               new MessageTemplateData {Type = type, SiteId = siteId};

            existingData.Data = JsonConvert.SerializeObject(messageTemplate);

            await _session.TransactAsync(session => session.SaveOrUpdateAsync(existingData));
        }

        private async Task<MessageTemplateData> GetExistingTemplateData(string type, int? siteId)
        {
            var query = _session.Query<MessageTemplateData>();
            if (siteId.HasValue)
            {
                return await
                    query
                        .Where(x => x.Type == type && x.SiteId == siteId)
                        .FirstOrDefaultAsync();
            }

            return await
                query
                    .Where(x => x.Type == type && x.SiteId == siteId)
                    .FirstOrDefaultAsync();
        }

        public async Task<T> GetNewMessageTemplate<T>() where T : MessageTemplate, new()
        {
            T template = null;
            var templateType = typeof(T);
            if (DefaultTemplateProviders.ContainsKey(templateType))
            {
                if (_serviceProvider.GetService(DefaultTemplateProviders[templateType]) is IGetDefaultMessageTemplate
                    getDefaultMessageTemplate)
                    template = await getDefaultMessageTemplate.Get() as T;
            }

            return template ?? new T();
        }
    }
}