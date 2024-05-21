using System;
using Microsoft.AspNetCore.Mvc;
using MrCMS.Entities.Multisite;

namespace MrCMS.Helpers
{
    public static class UrlHelperExtensions
    {
        /// <summary>
        /// Adds '/' to url if one doesn't exist.
        /// </summary>
        /// <param name="url"></param>
        /// <returns>/url</returns>
        public static string GetRelativeUrl(this string url)
        {
            return string.IsNullOrWhiteSpace(url) ? url : url.TrimStart('/').Insert(0, "/");
        }
        
        public static bool IsAbsoluteUrl(this string url)
        {
            var uri = new Uri(url, UriKind.RelativeOrAbsolute);
            return uri.IsAbsoluteUri;
        }
        
        public static string AbsoluteAction(this IUrlHelper helper, string action, string controller, object values)
        {
            return helper.Action(action, controller, values, helper.ActionContext.HttpContext.Request.Scheme);
        }

        public static string AbsoluteAction(this IUrlHelper helper, string action, string controller, object values,
            Site site)
        {
            return helper.Action(action, controller, values, helper.ActionContext.HttpContext.Request.Scheme,
                site.BaseUrl.Trim('/'));
        }


        public static string AbsoluteContent(this IUrlHelper url, string path)
        {
            var uri = new Uri(path, UriKind.RelativeOrAbsolute);

            //If the URI is not already absolute, rebuild it based on the current request.
            if (!uri.IsAbsoluteUri)
            {
                var request = url.ActionContext.HttpContext.Request;
                uri = new Uri(new Uri(request.Scheme + "://" + request.Host.Value), url.Content(path));
            }

            return uri.ToString();
        }
    }
}