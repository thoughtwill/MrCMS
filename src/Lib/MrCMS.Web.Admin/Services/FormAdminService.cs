using Microsoft.Extensions.Logging;
using MrCMS.Entities.Documents.Web;
using MrCMS.Entities.Documents.Web.FormProperties;
using MrCMS.Helpers;
using MrCMS.Models;
using MrCMS.Services.Resources;
using NHibernate;
using NHibernate.Criterion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MrCMS.Mapping;
using MrCMS.Web.Admin.Models;
using MrCMS.Web.Admin.Models.Forms;
using NHibernate.Linq;
using X.PagedList;

namespace MrCMS.Web.Admin.Services
{
    public class FormAdminService : IFormAdminService
    {
        private readonly ISession _session;
        private readonly IStringResourceProvider _stringResourceProvider;
        private readonly ILogger<FormAdminService> _logger;
        private readonly ISessionAwareMapper _mapper;
        private readonly IGetCurrentUserCultureInfo _getCurrentUserCultureInfo;

        public FormAdminService(ISession session, IStringResourceProvider stringResourceProvider,
            ILogger<FormAdminService> logger,
            ISessionAwareMapper mapper, IGetCurrentUserCultureInfo getCurrentUserCultureInfo)
        {
            _session = session;
            _stringResourceProvider = stringResourceProvider;
            _logger = logger;
            _mapper = mapper;
            _getCurrentUserCultureInfo = getCurrentUserCultureInfo;
        }

        public async Task ClearFormData(Form form)
        {
            await _session.TransactAsync(async session =>
            {
                foreach (var posting in form.FormPostings)
                {
                    await session.DeleteAsync(posting);
                }

                form.FormPostings.Clear();
            });
        }

        public async Task<byte[]> ExportFormData(Form form)
        {
            try
            {
                var stringBuilder = new StringBuilder();

                var headers = await GetHeadersForExport(form);
                stringBuilder.AppendLine(string.Join(",", headers.Select(FormatField)));

                var formDataForExport = await GetFormDataForExport(form);
                foreach (var data in formDataForExport)
                {
                    stringBuilder.AppendLine(string.Join(",", data.Value.Select(FormatField)));
                }

                return Encoding.Default.GetBytes(stringBuilder.ToString());
            }
            catch (Exception exception)
            {
                _logger.Log(LogLevel.Error, exception, exception.Message);
                throw;
            }
        }

        public async Task<FormPosting> DeletePosting(int id)
        {
            var posting = await _session.GetAsync<FormPosting>(id);
            posting.Form.FormPostings.Remove(posting);
            await _session.TransactAsync(session => session.DeleteAsync(posting));
            return posting;
        }

        public async Task<WebpageViewModel> GetPostingCountForWebpage(int webpageId)
        {
            return new()
            {
                FormId = await _session.Query<FormPosting>()
                    .Where(x => x.Webpage.Id == webpageId && x.Form != null)
                    .Select(x=> x.Form.Id).FirstOrDefaultAsync(),
                Count = await _session.Query<FormPosting>()
                    .WithOptions(options => options.SetCacheable(true))
                    .CountAsync(x => x.Webpage.Id == webpageId)
            };
        }

        public async Task<FormPosting> GetFormPosting(int id)
        {
            return await _session.GetAsync<FormPosting>(id);
        }

        public async Task SetOrders(List<SortItem> items)
        {
            await _session.TransactAsync(async session =>
            {
                foreach (var item in items)
                {
                    var formItem = await session.GetAsync<FormProperty>(item.Id);
                    formItem.DisplayOrder = item.Order;
                    await session.UpdateAsync(formItem);
                }
            });
        }

        public Task<IPagedList<Form>> Search(FormSearchModel model)
        {
            var query = _session.Query<Form>();

            if (!string.IsNullOrWhiteSpace(model.Name))
            {
                var parsed = int.TryParse(model.Name, out var id) && id > 0;
                if (parsed)
                {
                    query = query.Where(x => x.Id == id || x.Name.Contains(model.Name));
                }
                else
                {
                    query = query.Where(x => x.Name.Contains(model.Name));
                }
            }

            return query.PagedAsync(model.Page);
        }

        public async Task<Form> AddForm(AddFormModel model)
        {
            var form = _mapper.Map<Form>(model);

            await _session.TransactAsync(session => session.SaveAsync(form));

            return form;
        }

        public async Task<Form> GetForm(int id)
        {
            return await _session.GetAsync<Form>(id);
        }

        public async Task<UpdateFormModel> GetUpdateModel(int id)
        {
            var form = await GetForm(id);

            return _mapper.Map<UpdateFormModel>(form);
        }

        public async Task Update(UpdateFormModel model)
        {
            var form = await GetForm(model.Id);
            _mapper.Map(model, form);

            foreach (var o in model.Models)
            {
                _mapper.Map(o, form);
            }

            await _session.TransactAsync(session => session.UpdateAsync(form));
        }

        public async Task Delete(int id)
        {
            var form = await GetForm(id);

            await _session.TransactAsync(session => session.DeleteAsync(form));
        }

        public async Task<IPagedList<Webpage>> GetPagesWithForms(WebpagesWithEmbeddedFormQuery query)
        {
            return await _session.Query<FormPosting>()
                .Where(x => x.Webpage != null)
                .Select(x => x.Webpage)
                .OrderBy(x=>x.Name)
                .Distinct()
                .PagedAsync(query.Page, query.PageSize);
        }

        public async Task<PostingsModel> GetFormPostings(Form form, int page, string search)
        {
            FormPosting posting = null;
            var query =
                _session.QueryOver(() => posting).Where(() => posting.Form.Id == form.Id);
            if (!string.IsNullOrWhiteSpace(search))
            {
                query =
                    query.WithSubquery.WhereExists(
                        QueryOver.Of<FormValue>()
                            .Where(
                                value =>
                                    value.FormPosting.Id == posting.Id &&
                                    value.Value.IsInsensitiveLike(search, MatchMode.Anywhere))
                            .Select(value => value.Id));
            }

            var formPostings = await query.OrderBy(() => posting.CreatedOn).Desc.PagedAsync(page);

            return new PostingsModel(formPostings, form.Id);
        }

        private async Task<List<string>> GetHeadersForExport(Form form)
        {
            var headers = new List<string>();
            foreach (FormPosting posting in form.FormPostings)
            {
                headers.AddRange(posting.FormValues.Select(x => x.Key).Distinct());
            }

            headers.Add(await _stringResourceProvider.GetValue("Admin Form Postings Posted On", configureOptions => configureOptions.SetDefaultValue("Posted On")));
            return headers.Distinct().ToList();
        }

        private async Task<Dictionary<int, List<string>>> GetFormDataForExport(Form form)
        {
            var items = new Dictionary<int, List<string>>();
            var formatProvider = await _getCurrentUserCultureInfo.Get();
            var headersForExport = await GetHeadersForExport(form);
            for (int i = 0; i < form.FormPostings.Count; i++)
            {
                FormPosting posting = form.FormPostings[i];
                items.Add(i, new List<string>());
                foreach (FormValue value in headersForExport.SelectMany(header =>
                    posting.FormValues.Where(x => x.Key == header)))
                {
                    if (!value.IsFile)
                    {
                        items[i].Add(value.Value);
                    }
                    else
                    {
                        items[i].Add("http://" + form.Site.BaseUrl + value.Value);
                    }
                }

                items[i].Add(posting.CreatedOn.ToString(formatProvider));
            }

            return items.OrderByDescending(x => x.Value.Count).ToDictionary(pair => pair.Key, pair => pair.Value);
        }


        private string FormatField(string data)
        {
            return string.Format("{1}{0}{1}", (data ?? string.Empty).Replace(Quote, Quote + Quote), Quote);
        }

        public const string Quote = "\"";
    }
}
