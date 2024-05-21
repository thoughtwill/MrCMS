using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MrCMS.Website.Caching;
using NHibernate;

namespace MrCMS.Services
{
    public class GetActivePages : IGetActivePages
    {
        private readonly ISession _session;
        private readonly ICacheManager _cacheManager;

        public GetActivePages(ISession session, ICacheManager cacheManager)
        {
            _session = session;
            _cacheManager = cacheManager;
        }

        public async Task<IList<string>> GetActiveUrls(int pageId)
        {
            return await _cacheManager.GetOrCreateAsync($"active-pages-{pageId.ToString()}", () =>
            {
                var sql = @$"WITH Parents AS (
                      SELECT Id, '/' + [UrlSegment] as UrlSegment, ParentId, 0 AS Depth
                      FROM [Webpage]
                      WHERE [Id] = :pageId

                      UNION ALL

                      SELECT cat.Id, '/' + cat.[UrlSegment] as UrlSegment, cat.ParentId, p.Depth - 1
                      FROM [Webpage] cat, parents p
                      WHERE cat.Id = p.ParentId
                    )
                    SELECT [UrlSegment] FROM Parents";

                return _session.CreateSQLQuery(sql)
                    .AddScalar("UrlSegment", NHibernateUtil.String)
                    .SetParameter("pageId", pageId)
                    .ListAsync<string>();
                
            }, TimeSpan.FromMinutes(5), CacheExpiryType.Absolute);
            
        }

        public class Urls
        {
            public string UrlSegment { get; set; }
        }
    }
}