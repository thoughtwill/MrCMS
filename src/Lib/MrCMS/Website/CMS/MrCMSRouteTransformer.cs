using System;
using System.Net;
using System.Threading.Tasks;
using FluentNHibernate.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using MrCMS.Entities.Documents.Web;
using MrCMS.Services;
using MrCMS.Settings;

namespace MrCMS.Website.CMS
{
    public class MrCMSRouteTransformer : DynamicRouteValueTransformer
    {
        public const string IsPreview = "MrCMS-Preview";
        public const string IsCMSRequest = "MrCMS Router";

        public override async ValueTask<RouteValueDictionary> TransformAsync(HttpContext httpContext,
            RouteValueDictionary values)
        {
            // get the service provider 
            var serviceProvider = httpContext.RequestServices;

            // we only route requests that are of routable methods
            var method = httpContext.Request.Method;
            if (!serviceProvider.GetRequiredService<ICmsMethodTester>().IsRoutable(method))
            {
                return values;
            }

            //we don't need to lookup if the page is an admin page
            if (httpContext.Request.Path.StartsWithSegments("/admin") || httpContext.Request.Path.StartsWithSegments("/Admin")  || httpContext.Request.Path.StartsWithSegments("/notificationsHub") || httpContext.Request.Path.StartsWithSegments("/_blazor"))
            {
                return values;
            }

            // we want to look at the whole path with the leading '/' stripped
            var url = WebUtility.UrlDecode(httpContext.Request.Path.ToUriComponent()?.TrimStart('/'));

            var cmsRouteMatcher = serviceProvider.GetRequiredService<ICmsRouteMatcher>();
            var matchResult = await cmsRouteMatcher.TryMatch(url, method);

            // do homepage redirect check
            if (matchResult.PageData != null && !string.IsNullOrWhiteSpace(url))
            {
                var homePageId = serviceProvider.GetRequiredService<SiteSettings>().HomePageId;
                if (homePageId == 0)
                {
                    homePageId = await serviceProvider.GetRequiredService<ISetHomepage>().Set();
                }

                if (matchResult.PageData.Id == homePageId)
                {
                    return new RouteValueDictionary
                    {
                        ["controller"] = "Redirect",
                        ["action"] = "HomePage"
                    };
                }
            }

            return await HandleMatchResult(httpContext, values, serviceProvider, matchResult);
        }

        private static ValueTask<RouteValueDictionary> HandleMatchResult(HttpContext httpContext,
            RouteValueDictionary values,
            IServiceProvider serviceProvider, CmsMatchData matchResult,
            Action<RouteValueDictionary> assignAdditionalRouteData = null)
        {
            switch (matchResult.MatchType)
            {
                case CmsRouteMatchType.NoMatch:
                    return new ValueTask<RouteValueDictionary>(values);
                case CmsRouteMatchType.Preview:
                    httpContext.MakePreview();
                    break;
            }

            serviceProvider.GetRequiredService<ISetCurrentPage>()
                .SetPage(matchResult.PageData.Webpage);
            serviceProvider.GetRequiredService<IAssignPageDataToRouteValues>()
                .Assign(values, matchResult.PageData);

            assignAdditionalRouteData?.Invoke(values);

            httpContext.MakeCMSRequest();
            return new ValueTask<RouteValueDictionary>(values);
        }

        public static async ValueTask<RouteValueDictionary> HandlePageExecution(HttpContext httpContext,
            RouteValueDictionary values, Webpage webpage,
            Action<RouteValueDictionary> assignAdditionalRouteData = null)
        {
            var serviceProvider = httpContext.RequestServices;
            var matchResult = await serviceProvider.GetRequiredService<ICmsRouteMatcher>()
                .Match(webpage, httpContext.Request.Method);
            if (matchResult.WillRender)
                serviceProvider.GetService<ISetCurrentPage>().SetPage(webpage);
            return await HandleMatchResult(httpContext, values, serviceProvider, matchResult,
                assignAdditionalRouteData);
        }

        // private static async ValueTask<RouteValueDictionary> HandleUnauthorised(CmsMatchData matchData,
        //     RouteValueDictionary values, IServiceProvider serviceProvider)
        // {
        //
        //     var code = currentUser != null ? 403 : 401;
        //     values["controller"] = "Error";
        //     values["action"] = "HandleStatusCode";
        //     values["code"] = code;
        //     // httpContext.Response.Clear();
        //     // httpContext.Response.StatusCode = code;
        //     // await httpContext.Response.WriteAsync(code.ToString());
        //     return values;
        // }
    }
}
