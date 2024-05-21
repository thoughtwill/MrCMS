using System;
using System.Collections.Generic;
using System.Linq;
using MrCMS.Entities.Documents.Web;
using MrCMS.Helpers;
using MrCMS.Models;
using NHibernate;
using NHibernate.Transform;

namespace MrCMS.Services
{
    public class ExistAnyWebpageService : IExistAnyWebpageService
    {
        private readonly ISession _session;

        public ExistAnyWebpageService(ISession session)
        {
            _session = session;
        }

        private Dictionary<string, int> _counts;
        public bool ExistAny(Type type)
        {
            _counts = (_counts ?? GetCounts());
            return _counts[type.FullName] > 0;
        }

        private Dictionary<string, int> GetCounts()
        {
            WebpageCount countAlias = null;
            Webpage webpageAlias = null;
            IList<WebpageCount> webpageCounts = _session.QueryOver(() => webpageAlias)
                .SelectList(
                    builder =>
                        builder.SelectGroup(() => webpageAlias.WebpageTypeName)
                            .WithAlias(() => countAlias.Type)
                            .SelectCount(() => webpageAlias.Id)
                            .WithAlias(() => countAlias.Count)
                )
                .TransformUsing(Transformers.AliasToBean<WebpageCount>())
                .Cacheable()
                .List<WebpageCount>();

            foreach (
                Type type in
                    TypeHelper.GetAllConcreteMappedClassesAssignableFrom<Webpage>()
                        .Where(type => !webpageCounts.Select(count => count.Type).Contains(type.FullName)))
            {
                webpageCounts.Add(new WebpageCount {Type = type.FullName, Count = 0});
            }
            return webpageCounts.ToDictionary(count => count.Type, count => count.Count);
        }
    }
}