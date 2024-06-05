using System.Collections.Generic;
using MrCMS.Services.Caching;
using MrCMS.Website.Caching;
using NHibernate;
using NHibernate.Cache;
using NHibernate.Impl;

namespace MrCMS.Website
{
    public class ClearCachesService : IClearCachesService
    {
        private readonly ICacheManager _cacheManager;
        private readonly IEnumerable<IClearCache> _manualCacheClears;
        private readonly ISessionFactory _factory;
        private readonly IHighPriorityCacheManager _highPriorityCacheManager;

        public ClearCachesService(ICacheManager cacheManager, IEnumerable<IClearCache> manualCacheClears, ISessionFactory factory,
            IHighPriorityCacheManager highPriorityCacheManager)
        {
            _cacheManager = cacheManager;
            _manualCacheClears = manualCacheClears;
            _factory = factory;
            _highPriorityCacheManager = highPriorityCacheManager;
        }

        public void ClearCache()
        {
            _cacheManager.Clear();

            foreach (var cache in _manualCacheClears)
            {
                cache.ClearCache();
            }

            foreach (var (_, value) in (_factory as SessionFactoryImpl)?.GetAllSecondLevelCacheRegions()?? new Dictionary<string, ICache>())
            {
                value.Clear();
            }
        }

        public void ClearHighPriorityCache()
        {
            _highPriorityCacheManager.Clear();
        }
    }
}