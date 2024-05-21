using Microsoft.AspNetCore.Mvc.Rendering;
using MrCMS.Entities.Documents.Web;
using NHibernate;

namespace MrCMS.Helpers
{
    public static class WebpageExtensions
    {
        public static bool CanDeleteWebpage(this IHtmlHelper helper, int id)
        {
            return !helper.AnyChildren(id);
        }
        private static bool AnyChildren(this IHtmlHelper helper, int id)
        {
            var webpage = helper.GetRequiredService<ISession>().Get<Webpage>(id);
            if (webpage == null)
                return false;
            return AnyChildren(helper, webpage);
        }

        private static bool AnyChildren(this IHtmlHelper helper, Webpage webpage)
        {
            return helper.GetRequiredService<ISession>()
                .QueryOver<Webpage>()
                .Where(doc => doc.Parent != null && doc.Parent.Id == webpage.Id)
                .Cacheable()
                .Any();
        }
        // public static bool CanAddChildren(this Webpage webpage)
        // {
        //     return webpage.GetMetadata().ValidChildrenTypes.Any();
        // }
    }
}