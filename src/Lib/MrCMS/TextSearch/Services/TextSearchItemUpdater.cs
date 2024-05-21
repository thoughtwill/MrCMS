using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MrCMS.Entities;
using MrCMS.Helpers;
using MrCMS.TextSearch.Entities;
using MrCMS.TextSearch.EntityConverters;
using NHibernate;
using NHibernate.Linq;

namespace MrCMS.TextSearch.Services
{
    public class TextSearchItemUpdater : ITextSearchItemUpdater
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IStatelessSession _session;

        public TextSearchItemUpdater(IServiceProvider serviceProvider, IStatelessSession session)
        {
            _serviceProvider = serviceProvider;
            _session = session;
        }

        private async Task AddRecords(IEnumerable<TextSearchItem> searchItems)
        {
            var chunks = searchItems.Chunk(1000);
            foreach (var chunk in chunks)
            {
                await _session.TransactAsync(async session =>
                {
                    foreach (var item in chunk)
                    {
                        await session.InsertAsync(item);
                    }
                });
            }
        }


        private BaseTextSearchEntityConverter GetConverter(SystemEntity item)
        {
            var type = item.Unproxy()?.GetType();
            if (type == null)
                return null;
            if (!TextSearchConverterMap.Types.ContainsKey(type))
                return null;

            return _serviceProvider.GetRequiredService(TextSearchConverterMap.Types[type]) as
                BaseTextSearchEntityConverter;
        }

        public async Task Add(SystemEntity entity)
        {
            await Add(new List<SystemEntity> {entity});
        }

        public async Task Add(IEnumerable<SystemEntity> entities)
        {
            var items = new List<TextSearchItem>();
            foreach (var entity in entities)
            {
                var converter = GetConverter(entity);
                if (converter == null)
                    continue;
                var existing = await GetExisting(converter.BaseType.Name, entity.Id);
                if (existing != null)
                    continue;
                items.Add(new TextSearchItem
                {
                    Text = converter.GetText(entity),
                    DisplayName = converter.GetDisplayName(entity),
                    SystemType = entity.GetType().FullName,
                    EntityType = converter.BaseType.Name,
                    EntityId = entity.Id,
                    SiteId = (entity as SiteEntity)?.Site?.Id,
                    EntityCreatedOn = entity.CreatedOn,
                    EntityUpdatedOn = entity.UpdatedOn,
                });
            }

            await AddRecords(items);
        }

        public async Task Update(SystemEntity entity)
        {
            var converter = GetConverter(entity);
            if (converter == null)
                return;
            await UpdateExisting(converter, entity);
            // if (!
            //     )
            // {
            //     await Add(entity);
            // }
            // }
        }

        public async Task Delete(SystemEntity entity)
        {
            var converter = GetConverter(entity);
            if (converter == null)
                return;

            var type = converter.BaseType.Name;
            var id = entity.Id;
            await Delete(type, id);
        }

        public async Task Delete(IEnumerable<TextSearchItem> toDelete)
        {
            await _session.TransactAsync(async session =>
            {
                foreach (var item in toDelete)
                {
                    await session.DeleteAsync(item);
                }
            });
        }

        public async Task Delete(string type, int id)
        {
            var existing = await GetExisting(type, id);
            if (existing != null)
                await Delete(new[] {existing});
        }

        private async Task<TextSearchItem> GetExisting(string entityType, int entityId)
        {
            return await _session.Query<TextSearchItem>()
                .FirstOrDefaultAsync(x => x.EntityType == entityType && x.EntityId == entityId);
        }

        private async Task<bool> UpdateExisting(BaseTextSearchEntityConverter converter, SystemEntity entity)
        {
            var existing = await GetExisting(converter.BaseType.Name, entity.Id);
            if (existing == null)
                return false;
            existing.EntityCreatedOn = entity.CreatedOn;
            existing.EntityUpdatedOn = entity.UpdatedOn;
            existing.Text = converter.GetText(entity);
            existing.DisplayName = converter.GetDisplayName(entity);
            existing.SystemType = entity.GetType().FullName;
            await _session.TransactAsync(session => session.UpdateAsync(existing));
            return true;
        }
    }
}