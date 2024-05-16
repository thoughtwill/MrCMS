using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using MrCMS.Entities.Multisite;
using MrCMS.Entities.Resources;
using MrCMS.Helpers;
using MrCMS.Mapping;
using MrCMS.Services;
using MrCMS.Services.Resources;
using MrCMS.Settings;
using MrCMS.Web.Admin.Models;
using NHibernate;
using NHibernate.Linq;
using X.PagedList;

namespace MrCMS.Web.Admin.Services
{
    public class StringResourceAdminService : IStringResourceAdminService
    {
        private const string DefaultLanguage = "Default";
        private readonly IStringResourceProvider _provider;
        private readonly ISession _session;
        private readonly ISessionAwareMapper _mapper;
        private readonly IOptions<RequestLocalizationOptions> _requestLocalisationOptions;
        private readonly SiteSettings _siteSettings;
        private readonly IGetCurrentClaimsPrincipal _getCurrentClaimsPrincipal;

        public StringResourceAdminService(IStringResourceProvider provider, SiteSettings siteSettings,
            IGetCurrentClaimsPrincipal getCurrentClaimsPrincipal,
            ISession session, ISessionAwareMapper mapper, IOptions<RequestLocalizationOptions> requestLocalisationOptions)
        {
            _provider = provider;
            _siteSettings = siteSettings;
            _getCurrentClaimsPrincipal = getCurrentClaimsPrincipal;
            _session = session;
            _mapper = mapper;
            _requestLocalisationOptions = requestLocalisationOptions;
        }

        public async Task<IPagedList<StringResource>> Search(StringResourceSearchQuery searchQuery)
        {
            var allResources = await _provider.GetAllResources();
            IEnumerable<StringResource> resources =
                allResources.GetResourcesByKeyAndValue(searchQuery);
            if (searchQuery.Language == DefaultLanguage)
            {
                resources = resources.Where(resource => resource.UICulture == null);
            }
            else if (!string.IsNullOrWhiteSpace(searchQuery.Language))
            {
                resources = resources.Where(resource => resource.UICulture == searchQuery.Language);
            }

            if (searchQuery.SiteId.HasValue)
            {
                if (searchQuery.SiteId == -1)
                {
                    resources = resources.Where(resource => resource.Site == null);
                }
                else if (searchQuery.SiteId > 0)
                {
                    resources =
                        resources.Where(resource => resource.Site != null && resource.Site.Id == searchQuery.SiteId);
                }
            }

            return new PagedList<StringResource>(
                resources.OrderBy(resource => StringResourceExtensions.GetDisplayKey(resource.Key)), searchQuery.Page,
                _siteSettings.DefaultPageSize);
        }

        public async Task Add(AddStringResourceModel model)
        {
            var resource = _mapper.Map<StringResource>(model);
            await _provider.AddOverride(resource);
        }

        public async Task<StringResource> GetResource(int id)
        {
            return await _session.GetAsync<StringResource>(id);
        }
        
        public async Task<StringResource> GetResource(string key)
        {
            var user = await _getCurrentClaimsPrincipal.GetPrincipal();
            var currentUiCulture = user?.GetUserCulture();
            return await _session.Query<StringResource>()
                .FirstOrDefaultAsync(f => f.Key == key && f.UICulture == currentUiCulture);
        }

        public UpdateStringResourceModel GetEditModel(StringResource resource)
        {
            return _mapper.Map<UpdateStringResourceModel>(resource);
        }

        public async Task Update(UpdateStringResourceModel model)
        {
            var resource = await GetResource(model.Id);
            _mapper.Map(model, resource);
            await _provider.Update(resource);
        }

        public async Task<SaveResult> Update(StringResourceInlineUpdateModel model)
        {
            var resource = await GetResource(model.Key);
            if (resource != null)
            {
                resource.Value = model.Value;
                await _provider.Update(resource);
                return new SaveResult
                {
                    success = true
                };
            }

            return new SaveResult
            {
                message = "Resource key is not valid",
                success = false
            };
        }

        public async Task Delete(int id)
        {
            var resource = await GetResource(id);
            await _provider.Delete(resource);
        }

        public async Task<List<SelectListItem>> GetLanguageOptions(string key, int? siteId)
        {
            List<CultureInfo> cultureInfos = _requestLocalisationOptions.Value.SupportedCultures?.ToList();
            IEnumerable<string> languages = await _provider.GetOverriddenLanguages(key, siteId);
            cultureInfos.RemoveAll(info => languages.Contains(info.Name));
            return cultureInfos.OrderBy(info => info.DisplayName)
                .BuildSelectItemList(info => info.DisplayName, info => info.Name,
                    info => info.Name == _siteSettings.UICulture,
                    SelectListItemHelper.EmptyItem("Select a culture..."));
        }

        public async Task<List<SelectListItem>> SearchLanguageOptions()
        {
            List<CultureInfo> cultureInfos = CultureInfo.GetCultures(CultureTypes.SpecificCultures).ToList();
            IEnumerable<string> languages = await _provider.GetOverriddenLanguages();
            cultureInfos = cultureInfos.FindAll(info => languages.Contains(info.Name));

            List<SelectListItem> selectListItems = cultureInfos.OrderBy(info => info.DisplayName)
                .BuildSelectItemList(info => info.DisplayName, info => info.Name, emptyItem: null);

            selectListItems.Insert(0, new SelectListItem { Text = "Any", Value = "" });
            selectListItems.Insert(1, new SelectListItem { Text = DefaultLanguage, Value = DefaultLanguage });
            return selectListItems;
        }

        public async Task<AddStringResourceModel> GetNewResource(string key, int? id)
        {
            var allResources = await _provider.GetAllResources();
            string value =
                allResources.Where(x => x.Key == key && x.Site == null && x.UICulture == null)
                    .Select(resource => resource.Value)
                    .FirstOrDefault();
            return new AddStringResourceModel { Key = key, SiteId = id, Value = value };
        }

        public async Task<List<SelectListItem>> ChooseSiteOptions(ChooseSiteParams chooseSiteParams)
        {
            var allResources = await _provider.GetAllResources();
            IEnumerable<StringResource> resourcesByKey =
                allResources.Where(x => x.Key == chooseSiteParams.Key);
            List<Site> sites = await GetAllSites();

            if (!chooseSiteParams.Language)
            {
                resourcesByKey = resourcesByKey.Where(resource => resource.Site != null && resource.UICulture == null);
                sites =
                    sites.Where(site => !resourcesByKey.Select(resource => resource.Site.Id).Contains(site.Id))
                        .ToList();
            }

            return sites
                .BuildSelectItemList(site => site.DisplayName, site => site.Id.ToString(), emptyItem: null);
        }

        public async Task<List<SelectListItem>> SearchSiteOptions()
        {
            var allSites = await GetAllSites();
            List<SelectListItem> siteOptions = allSites
                .BuildSelectItemList(site => site.DisplayName, site => site.Id.ToString(), emptyItemText: "All");

            siteOptions.Insert(1, new SelectListItem
            {
                Text = "System Default",
                Value = "-1"
            });

            return siteOptions;
        }

        private async Task<List<Site>> GetAllSites()
        {
            return await _session.Query<Site>()
                .OrderBy(x => x.Name)
                .WithOptions(x => x.SetCacheable(true))
                .ToListAsync();
        }
    }
}
