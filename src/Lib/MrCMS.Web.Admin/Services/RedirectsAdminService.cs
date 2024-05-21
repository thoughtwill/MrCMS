using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MrCMS.Entities.Documents.Web;
using MrCMS.Helpers;
using MrCMS.Services;
using MrCMS.Web.Admin.Helpers;
using MrCMS.Web.Admin.Models;
using MrCMS.Web.Admin.Models.Redirects;
using NHibernate;
using NHibernate.Linq;
using X.PagedList;

namespace MrCMS.Web.Admin.Services
{
    public class RedirectsAdminService : IRedirectsAdminService
    {
        private readonly ISession _session;
        private readonly IGetHomePage _getHomePage;

        public RedirectsAdminService(ISession session, IGetHomePage getHomePage)
        {
            _session = session;
            _getHomePage = getHomePage;
        }

        public async Task<IPagedList<UrlHistory>> SearchAll(RedirectsSearchQuery searchQuery)
        {
            return await Search(searchQuery);
        }

        public async Task<IPagedList<UrlHistory>> SearchKnown404s(Known404sSearchQuery searchQuery)
        {
            return await Search(searchQuery);
        }

        private async Task<IPagedList<UrlHistory>> Search(IRedirectsSearchQuery searchQuery)
        {
            IQueryable<UrlHistory> query = _session.Query<UrlHistory>()
                .Fetch(x => x.Webpage);
            // .Where(x => x.Webpage == null && x.RedirectUrl == null)


            if (!string.IsNullOrWhiteSpace(searchQuery.Url))
                query = query.Where(x => x.UrlSegment.Like($"%{searchQuery.Url}%"));

            query = searchQuery.Type switch
            {
                UrlHistoryType.ToWebpage => query.Where(x => x.Webpage != null),
                UrlHistoryType.ToUrl => query.Where(x => x.RedirectUrl != null),
                UrlHistoryType.Gone => query.Where(x => x.IsGone),
                UrlHistoryType.Ignored => query.Where(x => x.IsIgnored),
                UrlHistoryType.Unhandled => query.Where(x =>
                    x.Webpage == null && x.RedirectUrl == null && !x.IsGone && !x.IsIgnored),
                _ => query
            };

            query = searchQuery.SortBy switch
            {
                RedirectSortBy.Latest => query.OrderByDescending(x => x.CreatedOn),
                RedirectSortBy.FailedLookupCount => query.OrderByDescending(x => x.FailedLookupCount),
                _ => query
            };

            return await query.PagedAsync(searchQuery.Page);
        }

        public async Task<UrlHistory> GetUrlHistory(int id)
        {
            return await _session.GetAsync<UrlHistory>(id);
        }

        public async Task MarkAsGone(int id)
        {
            var urlHistory = await GetUrlHistory(id);
            urlHistory.IsGone = true;
            await Update(urlHistory);
        }

        public async Task MarkAsIgnored(int id)
        {
            var urlHistory = await GetUrlHistory(id);
            urlHistory.IsIgnored = true;
            await Update(urlHistory);
        }

        public async Task Reset(int id)
        {
            var urlHistory = await GetUrlHistory(id);
            urlHistory.IsGone = false;
            urlHistory.IsIgnored = false;
            urlHistory.Webpage?.Urls?.Remove(urlHistory);
            urlHistory.Webpage = null;
            urlHistory.RedirectUrl = null;
            await Update(urlHistory);
        }

        public async Task Remove(int id)
        {
            var urlHistory = await GetUrlHistory(id);
            urlHistory.Webpage?.Urls?.Remove(urlHistory);
            await _session.TransactAsync(session => session.DeleteAsync(urlHistory));
        }

        public Task<SetRedirectUrlModel> GetSetRedirectUrlModel(int id)
        {
            return Task.FromResult<SetRedirectUrlModel>(new() { Id = id });
        }

        public async Task SetRedirectUrl(SetRedirectUrlModel model)
        {
            var urlHistory = await GetUrlHistory(model.Id);
            urlHistory.RedirectUrl = model.Url;
            await Update(urlHistory);
        }

        public Task<SetRedirectPageModel> GetSetRedirectPageModel(int id)
        {
            return Task.FromResult<SetRedirectPageModel>(new() { Id = id });
        }

        public async Task SetRedirectPage(SetRedirectPageModel model)
        {
            var urlHistory = await GetUrlHistory(model.Id);
            Webpage webpage = await _session.GetAsync<Webpage>(model.PageId);
            if (webpage == null)
                return;

            webpage.Urls.Add(urlHistory);
            urlHistory.Webpage = webpage;
            await Update(urlHistory);
        }

        public async Task<RedirectImportResult> ImportRedirects(Stream fileStream)
        {
            var redirects = MrCMS.Helpers.CsvHelper.ReadCsv<RedirectImportModel>(fileStream, skipCount: 1);

            var validRedirects = redirects.GetValidRedirects(out var errorList);

            //import url
            await _session.TransactAsync(async session =>
            {
                foreach (var importData in validRedirects)
                {
                    var newUrlSegment = importData.NewUrl.Trim('/');
                    var oldUrlSegment = importData.OldUrl.Trim('/');


                    //Check for Unique url
                    var isInRedirect = await _session.Query<Redirect>().AnyAsync(f => f.UrlSegment == oldUrlSegment);
                    var isInUrlHistory =
                        await _session.Query<UrlHistory>().AnyAsync(f => f.UrlSegment == oldUrlSegment);

                    if (isInRedirect || isInUrlHistory)
                    {
                        errorList.Add(new RedirectImportErrorModel(importData)
                        {
                            Error = RedirectImportErrorType.NotUniqueUrl,
                        });
                        continue;
                    }

                    var webpage = string.IsNullOrWhiteSpace(newUrlSegment)
                        ? await _getHomePage.Get()
                        : await _session.QueryOver<Webpage>().Where(x => x.UrlSegment == newUrlSegment)
                            .SingleOrDefaultAsync();

                    var urlHistory = new UrlHistory
                    {
                        Webpage = webpage,
                        UrlSegment = oldUrlSegment,
                        RedirectUrl = newUrlSegment
                    };
                    await session.SaveAsync(urlHistory);

                    webpage?.Urls.Add(urlHistory);
                }
            });

            return new RedirectImportResult
            {
                ImportedCount = validRedirects.Count,
                Errors = errorList
            };
        }

        private async Task Update(UrlHistory urlHistory)
        {
            await _session.TransactAsync(session => session.UpdateAsync(urlHistory));
        }
    }
}