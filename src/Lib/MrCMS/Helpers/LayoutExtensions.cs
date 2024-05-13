using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Rendering;
using MrCMS.Entities.Documents.Layout;
using NHibernate;

namespace MrCMS.Helpers
{
    public static class LayoutExtensions
    {
        public static bool CanDeleteLayout(this IHtmlHelper helper, int id)
        {
            return !helper.AnyChildren(id);
        }
        private static bool AnyChildren(this IHtmlHelper helper, int id)
        {
            var layout = helper.GetRequiredService<ISession>().Get<Layout>(id);
            if (layout == null)
                return false;
            return AnyChildren(helper, layout);
        }

        private static bool AnyChildren(this IHtmlHelper helper, Layout layout)
        {
            return helper.GetRequiredService<ISession>()
                .QueryOver<Layout>()
                .Where(doc => doc.Parent != null && doc.Parent.Id == layout.Id)
                .Cacheable()
                .Any();
        }
        public static IEnumerable<LayoutArea> GetLayoutAreas(this Layout layout)
        {
            Layout current = layout;
            var layoutAreas = new List<LayoutArea>();
            while (current != null)
            {
                layoutAreas.AddRange(current.LayoutAreas);
                current = current.Parent.Unproxy() as Layout;
            }

            return layoutAreas;
        }

        public static string GetLayoutName(this Layout layout)
        {
            return !string.IsNullOrWhiteSpace(layout.Path)
                ? layout.Path
                : string.Format("_{0}", Regex.Replace(layout.Name, @"[^\w//]+", ""));
        }
    }
}