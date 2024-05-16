using System.Threading.Tasks;
using MrCMS.Entities.Documents.Web;
using MrCMS.Helpers;
using MrCMS.Mapping;
using MrCMS.Web.Admin.Models;
using NHibernate;

namespace MrCMS.Web.Admin.Services
{
    public class UrlHistoryAdminService : IUrlHistoryAdminService
    {
        private readonly ISession _session;
        private readonly ISessionAwareMapper _mapper;

        public UrlHistoryAdminService(ISession session, ISessionAwareMapper mapper)
        {
            _session = session;
            _mapper = mapper;
        }

        public async Task<UrlHistory> Delete(int id)
        {
            var urlHistory = _session.Get<UrlHistory>(id);
            if (urlHistory == null)
            {
                return null;
            }

            urlHistory.Webpage?.Urls.Remove(urlHistory);
            await _session.TransactAsync(session => session.DeleteAsync(urlHistory));
            return urlHistory;
        }

        public async Task Add(AddUrlHistoryModel model)
        {
            var urlHistory = _mapper.Map<UrlHistory>(model);
            urlHistory.Webpage?.Urls.Add(urlHistory);
            await _session.TransactAsync(session => session.SaveAsync(urlHistory));
        }

        public AddUrlHistoryModel GetUrlHistoryToAdd(int webpageId)
        {
            return new AddUrlHistoryModel
            {
                WebpageId = webpageId
            };
        }
    }
}
