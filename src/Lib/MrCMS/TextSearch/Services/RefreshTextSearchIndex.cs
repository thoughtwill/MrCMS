using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using MrCMS.Entities;
using MrCMS.Helpers;
using MrCMS.TextSearch.Entities;
using MrCMS.TextSearch.EntityConverters;
using NHibernate;
using NHibernate.Linq;

namespace MrCMS.TextSearch.Services
{
    public class RefreshTextSearchIndex : IRefreshTextSearchIndex
    {
        private readonly IStatelessSession _statelessSession;
        private readonly ISession _session;
        private readonly IServiceProvider _serviceProvider;
        private readonly ITextSearchItemUpdater _updater;

        public RefreshTextSearchIndex(
            IStatelessSession statelessSession,
            ISession session,
            IServiceProvider serviceProvider,
            ITextSearchItemUpdater updater)
        {
            _statelessSession = statelessSession;
            _session = session;
            _serviceProvider = serviceProvider;
            _updater = updater;
        }

        [DisableConcurrentExecution(timeoutInSeconds: 3600)]
        public async Task Refresh()
        {
            var converterTypes = TextSearchConverterMap.Types.Values.Distinct().ToList();

            var converters = converterTypes.Select(x => _serviceProvider.GetRequiredService(x))
                .Cast<BaseTextSearchEntityConverter>().ToList();

            var baseEntityTypes = converters.Select(x => x.BaseType).Distinct().ToList();

            var allToAdd = new List<SystemEntity>();
            var allToUpdate = new List<SystemEntity>();
            var allToDelete = new List<TextSearchItem>();


            foreach (var entityType in baseEntityTypes)
            {
                HashSet<SystemEntity> all = (await _statelessSession.CreateCriteria(entityType).ListAsync())
                    .Cast<SystemEntity>()
                    .Select(x => x.Unproxy())
                    .Where(x => x != null) //rm deleted
                    .ToHashSet();

                var existingIds = all.Select(x => x.Id).ToHashSet();
                var entityName = entityType.Name;
                var existingEntries = await _statelessSession.Query<TextSearchItem>()
                    .Where(x => x.EntityType == entityName)
                    .ToListAsync();
                var existingEntryIds = existingEntries.Select(x => x.EntityId).ToHashSet();
                var existingEntryLookup = existingEntries.ToDictionary(x => x.EntityId);

                var toAdd = all.Where(x => !existingEntryIds.Contains(x.Id)).ToHashSet();

                var toUpdate = all.Except(toAdd);
                // only update entries with different UpdatedOn
                toUpdate = toUpdate.Where(x => x.UpdatedOn != existingEntryLookup[x.Id].EntityUpdatedOn).ToList();

                var toDelete = existingEntries.Where(x => !existingIds.Contains(x.EntityId)).ToHashSet();

                allToAdd.AddRange(toAdd);
                allToUpdate.AddRange(toUpdate);
                allToDelete.AddRange(toDelete);
            }

            await _updater.Add(allToAdd);
            foreach (var entity in allToUpdate) await _updater.Update(entity);
            await _updater.Delete(allToDelete);
        }
    }

    public interface IRefreshTextSearchIndex
    {
        Task Refresh();
    }
}