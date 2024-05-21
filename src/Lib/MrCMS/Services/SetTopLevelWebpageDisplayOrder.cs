using System.Linq;
using System.Threading.Tasks;
using MrCMS.Entities.Documents.Web;
using MrCMS.Events;

namespace MrCMS.Services
{
    public class SetTopLevelWebpageDisplayOrder : IOnAdding<Webpage>
    {
        private readonly IGetWebpagesByParent _getWebpagesByParent;

        public SetTopLevelWebpageDisplayOrder(IGetWebpagesByParent getWebpagesByParent)
        {
            _getWebpagesByParent = getWebpagesByParent;
        }

        public async Task Execute(OnAddingArgs<Webpage> args)
        {
            var webpage = args.Item;
            // if the document isn't set or it's not top level (i.e. has a parent) we don't want to deal with it here
            if (webpage is not { Parent: null })
                return;

            // if it's not 0 it means it's been set, so we'll not update it
            if (webpage.DisplayOrder != 0)
                return;

            var webpagesByParent = (await _getWebpagesByParent.GetWebpages(null))
                .Where(doc => doc != webpage).ToList();

            webpage.DisplayOrder = webpagesByParent.Any()
                ? webpagesByParent.Max(category => category.DisplayOrder) + 1
                : 0;
        }
    }
}