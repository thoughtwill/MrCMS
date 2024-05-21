using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MrCMS.Entities.Multisite;
using MrCMS.Helpers;
using MrCMS.Mapping;
using MrCMS.Models;
using MrCMS.Services.CloneSite;
using MrCMS.Web.Admin.Models;
using NHibernate;
using NHibernate.Linq;

namespace MrCMS.Web.Admin.Services
{
    public class SiteAdminService : ISiteAdminService
    {
        private readonly ICloneSiteService _cloneSiteService;
        private readonly ISessionAwareMapper _mapper;
        private readonly ISession _session;

        public SiteAdminService(ISession session, ICloneSiteService cloneSiteService, ISessionAwareMapper mapper)
        {
            _session = session;
            _cloneSiteService = cloneSiteService;
            _mapper = mapper;
        }

        public async Task<IList<Site>> GetAllSites()
        {
            return await _session.QueryOver<Site>().OrderBy(site => site.Name).Asc.Cacheable().ListAsync();
        }

        public async Task<Site> GetSite(int id)
        {
            return await _session.GetAsync<Site>(id);
        }

        public async Task AddSite(AddSiteModel model, List<SiteCopyOption> options)
        {
            var site = _mapper.Map<Site>(model);

            await _session.TransactAsync(session => session.SaveAsync(site));

            await _cloneSiteService.CloneData(site, options);
        }

        public async Task<UpdateSiteModel> GetEditModel(int id)
        {
            var site = await GetSite(id);

            return _mapper.Map<UpdateSiteModel>(site);
        }

        public async Task<IList<RedirectedDomain>> GetRedirectedDomains(int id)
        {
            return await _session.Query<RedirectedDomain>()
                .Where(x => x.Site.Id == id)
                .OrderBy(x => x.CreatedOn)
                .ToListAsync();
        }

        public async Task SaveSite(UpdateSiteModel model)
        {
            var site = await GetSite(model.Id);
            _mapper.Map(model, site);
            await _session.TransactAsync(session => session.UpdateAsync(site));
        }

        public async Task DeleteSite(int id)
        {
            var site = await GetSite(id);
            await _session.TransactAsync(session => session.DeleteAsync(site));
        }
    }
}