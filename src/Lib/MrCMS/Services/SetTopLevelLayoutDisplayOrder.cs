using System.Linq;
using System.Threading.Tasks;
using MrCMS.Entities.Documents.Layout;
using MrCMS.Events;

namespace MrCMS.Services
{
    public class SetTopLevelLayoutDisplayOrder : IOnAdding<Layout>
    {
        private readonly IGetLayoutsByParent _getLayoutsByParent;

        public SetTopLevelLayoutDisplayOrder(IGetLayoutsByParent getLayoutsByParent)
        {
            _getLayoutsByParent = getLayoutsByParent;
        }

        public async Task Execute(OnAddingArgs<Layout> args)
        {
            var webpage = args.Item;
            // if the document isn't set or it's not top level (i.e. has a parent) we don't want to deal with it here
            if (webpage is not { Parent: null })
                return;

            // if it's not 0 it means it's been set, so we'll not update it
            if (webpage.DisplayOrder != 0)
                return;

            var layoutsByParent = (await _getLayoutsByParent.GetLayouts(null))
                .Where(doc => doc != webpage).ToList();

            webpage.DisplayOrder = layoutsByParent.Any()
                ? layoutsByParent.Max(category => category.DisplayOrder) + 1
                : 0;
        }
    }
}