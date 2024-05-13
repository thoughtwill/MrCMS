using System.Linq;
using System.Threading.Tasks;
using MrCMS.Entities.Documents.Media;
using MrCMS.Events;

namespace MrCMS.Services
{
    public class SetTopLevelMediaCategoryDisplayOrder : IOnAdding<MediaCategory>
    {
        private readonly IGetMediaCategoriesByParent _getMediaCategoriesByParent;

        public SetTopLevelMediaCategoryDisplayOrder(IGetMediaCategoriesByParent getMediaCategoriesByParent)
        {
            _getMediaCategoriesByParent = getMediaCategoriesByParent;
        }

        public async Task Execute(OnAddingArgs<MediaCategory> args)
        {
            var webpage = args.Item;
            // if the document isn't set or it's not top level (i.e. has a parent) we don't want to deal with it here
            if (webpage is not { Parent: null })
                return;

            // if it's not 0 it means it's been set, so we'll not update it
            if (webpage.DisplayOrder != 0)
                return;

            var mediaCategoriesByParent = (await _getMediaCategoriesByParent.GetMediaCategories(null))
                .Where(doc => doc != webpage).ToList();

            webpage.DisplayOrder = mediaCategoriesByParent.Any()
                ? mediaCategoriesByParent.Max(category => category.DisplayOrder) + 1
                : 0;
        }
    }
}