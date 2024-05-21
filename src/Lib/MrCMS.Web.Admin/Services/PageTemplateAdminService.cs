using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using MrCMS.Apps;
using MrCMS.Entities.Documents.Layout;
using MrCMS.Entities.Documents.Web;
using MrCMS.Helpers;
using MrCMS.Mapping;
using MrCMS.Web.Admin.Models;
using NHibernate;
using X.PagedList;

namespace MrCMS.Web.Admin.Services
{
    public class PageTemplateAdminService : IPageTemplateAdminService
    {
        private readonly IGetUrlGeneratorOptions _getUrlGeneratorOptions;
        private readonly ISessionAwareMapper _mapper;
        private readonly MrCMSAppContext _appContext;
        private readonly ISession _session;
        private static HashSet<Type> _webpageTypes;

        public PageTemplateAdminService(ISession session, IGetUrlGeneratorOptions getUrlGeneratorOptions,
            ISessionAwareMapper mapper, MrCMSAppContext appContext)
        {
            _session = session;
            _getUrlGeneratorOptions = getUrlGeneratorOptions;
            _mapper = mapper;
            _appContext = appContext;
        }

        public async Task<IPagedList<PageTemplate>> Search(PageTemplateSearchQuery query)
        {
            IQueryOver<PageTemplate, PageTemplate> queryOver = _session.QueryOver<PageTemplate>();

            return await queryOver.PagedAsync(query.Page);
        }

        public async Task Add(AddPageTemplateModel model)
        {
            var template = _mapper.Map<PageTemplate>(model);
            await _session.TransactAsync(session => session.SaveAsync(template));
        }

        public async Task<UpdatePageTemplateModel> GetEditModel(int id)
        {
            var template = await GetTemplate(id);
            return _mapper.Map<UpdatePageTemplateModel>(template);
        }

        public async Task Update(UpdatePageTemplateModel model)
        {
            var template = await GetTemplate(model.Id);
            _mapper.Map(model, template);
            await _session.TransactAsync(session => session.UpdateAsync(template));
        }

        public async Task Delete(int id)
        {
            var template = await GetTemplate(id);
            await _session.TransactAsync(session => session.DeleteAsync(template));
        }

        private async Task<PageTemplate> GetTemplate(int id)
        {
            return await _session.GetAsync<PageTemplate>(id);
        }

        public List<SelectListItem> GetPageTypeOptions()
        {
            List<SelectListItem> selectListItems = GetNewList();
            selectListItems.AddRange(GetWebpageTypes()
                .Select(type => new
                {
                    typeName = type.FullName, displayName = type.Name.BreakUpString(),
                    app = _appContext.Types.ContainsKey(type) ? _appContext.Types[type].Name : "System"
                })
                .OrderBy(x => x.app)
                .ThenBy(x => x.displayName)
                .Select(info => new SelectListItem
                {
                    Text = $"{info.displayName} ({info.app})",
                    Value = info.typeName
                }));
            return selectListItems;
        }

        private static HashSet<Type> GetWebpageTypes()
        {
            return _webpageTypes ??= TypeHelper.GetAllConcreteMappedClassesAssignableFrom<Webpage>();
        }

        public async Task<List<SelectListItem>> GetLayoutOptions()
        {
            var layouts =
                await _session.QueryOver<Layout>().Where(x => x.Hidden == false).Cacheable().ListAsync();

            return layouts.BuildSelectItemList(layout => layout.Name,
                layout => layout.Id.ToString(CultureInfo.InvariantCulture),
                emptyItemText: "Please select...");
        }

        public List<SelectListItem> GetUrlGeneratorOptions(string typeName)
        {
            var type = TypeHelper.GetTypeByName(typeName);
            List<SelectListItem> urlGeneratorOptions = _getUrlGeneratorOptions.Get(type);
            urlGeneratorOptions.Insert(0, new SelectListItem {Text = "Please select...", Value = ""});
            return urlGeneratorOptions;
        }

        private static List<SelectListItem> GetNewList()
        {
            var selectListItems = new List<SelectListItem>
            {
                new SelectListItem {Text = "Please select...", Value = ""}
            };
            return selectListItems;
        }
    }
}