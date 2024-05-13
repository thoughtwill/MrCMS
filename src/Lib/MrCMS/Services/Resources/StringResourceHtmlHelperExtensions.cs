using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using MrCMS.Website;

namespace MrCMS.Services.Resources
{
    public static class StringResourceHtmlHelperExtensions
    {
        public static async Task<IHtmlContent> Resource(this IHtmlHelper helper, string key,
            Action<ResourceOptions> configureOptions = null)
        {
            return await Resource(helper, key, key, configureOptions);
        }

        public static async Task<IHtmlContent> Resource(this IHtmlHelper helper, string key, string defaultValue,
            Action<ResourceOptions> configureOptions = null)
        {
            var previousOptions = configureOptions;
            var editingEnabled = await helper.EditingEnabled();
            configureOptions = options =>
            {
                // set enabled from setting if possible
                if (editingEnabled)
                    options.EnableInlineEditing();

                // set default value based on passed value if possible
                if (!string.IsNullOrWhiteSpace(defaultValue))
                    options.SetDefaultValue(defaultValue);

                previousOptions?.Invoke(options);
            };

            return await helper.ViewContext.HttpContext.RequestServices.GetRequiredService<IStringResourceProvider>()
                .GetValue(key, configureOptions);
        }

        public static async Task<string> PlainResource(this IHtmlHelper helper, string key,
            Action<PlainResourceOptions> configureOptions = null)
        {
            return await PlainResource(helper, key, key, configureOptions);
        }

        public static async Task<string> PlainResource(this IHtmlHelper helper, string key, string defaultValue,
            Action<PlainResourceOptions> configureOptions = null)
        {
            var previousOptions = configureOptions;
            configureOptions = options =>
            {
                // set default value based on passed value if possible
                if (!string.IsNullOrWhiteSpace(defaultValue))
                    options.SetDefaultValue(defaultValue);

                previousOptions?.Invoke(options);
            };

            return await helper.ViewContext.HttpContext.RequestServices.GetRequiredService<IStringResourceProvider>()
                .GetValue(key, configureOptions);
        }
    }
}