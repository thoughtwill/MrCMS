﻿using MrCMS.Entities.Documents.Web;
using MrCMS.Helpers;
using MrCMS.Web.Apps.Admin.Models;

using X.PagedList;

namespace MrCMS.Web.Apps.Admin.Services
{
    public class CustomScriptPageAdminService : ICustomScriptPageAdminService
    {
        private readonly ISession _session;

        public CustomScriptPageAdminService(ISession session)
        {
            _session = session;
        }

        public IPagedList<Webpage> Search(CustomScriptPagesSearchModel searchModel)
        {
            return _session.QueryOver<Webpage>()
                .Where(x =>
                    (x.CustomHeaderScripts != null && x.CustomHeaderScripts != "")
                    ||
                    (x.CustomFooterScripts != null && x.CustomFooterScripts != "")
                )
                .OrderBy(x => x.Name).Asc
                .Paged(searchModel.Page);
        }
    }
}