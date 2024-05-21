using System.Collections.Generic;
using System.Linq;
using MrCMS.Helpers;
using MrCMS.Web.Admin.Models.Redirects;

namespace MrCMS.Web.Admin.Helpers;

public static class RedirectImportModelExtensions
{
    public static List<RedirectImportModel> GetValidRedirects(this List<RedirectImportModel> list,
        out List<RedirectImportErrorModel> errors)
    {
        if ((list?.Count ?? 0) == 0)
        {
            errors = new List<RedirectImportErrorModel>();
            return new List<RedirectImportModel>();
        }

        var validList = new List<RedirectImportModel>();

        errors = new List<RedirectImportErrorModel>();
        foreach (var item in list)
        {
            //Check for empty url
            if (string.IsNullOrWhiteSpace(item.OldUrl) || string.IsNullOrWhiteSpace(item.NewUrl))
            {
                errors.Add(new RedirectImportErrorModel(item)
                {
                    Error = RedirectImportErrorType.EmptyUrl,
                });
            }
            //Check for absolute url
            else if (item.OldUrl.IsAbsoluteUrl() || item.NewUrl.IsAbsoluteUrl())
            {
                errors.Add(new RedirectImportErrorModel(item)
                {
                    Error = RedirectImportErrorType.AbsoluteUrl
                });
            }
            //check for none-unique url
            else if (AreEqualUrl(item.OldUrl, item.NewUrl))
            {
                errors.Add(new RedirectImportErrorModel(item)
                {
                    Error = RedirectImportErrorType.NotUniqueUrl
                });
            }
            //check for duplicated urls (list only)
            else if (list.Count(f => AreEqualUrl(f.OldUrl, item.OldUrl)) > 1)
            {
                errors.Add(new RedirectImportErrorModel(item)
                {
                    Error = RedirectImportErrorType.DuplicatedUrl
                });
            }
            else
            {
                validList.Add(item);
            }
        }

        return validList;
    }

    private static bool AreEqualUrl(string path1, string path2)
    {
        return path1.Trim('/') == path2.Trim('/');
    }
}