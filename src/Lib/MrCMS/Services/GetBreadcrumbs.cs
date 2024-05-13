using System.Collections.Generic;
using System.Threading.Tasks;
using MrCMS.Entities.Documents;
using MrCMS.Entities.Documents.Web;
using NHibernate;

namespace MrCMS.Services
{
    public class GetBreadcrumbs : IGetBreadcrumbs
    {
        private readonly ISession _session;

        public GetBreadcrumbs(ISession session)
        {
            _session = session;
        }

        public async Task<IReadOnlyList<Webpage>> Get(int? parent)
        {
            var list = new List<Webpage>();
            if (parent > 0)
            {
                var webpage = await _session.GetAsync<Webpage>(parent);
                while (webpage != null)
                {
                    list.Add(webpage);
                    webpage = webpage.Parent;
                }
            }

            list.Reverse();
            return list;
        }
    }
}