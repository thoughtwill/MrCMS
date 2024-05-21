using MrCMS.Web.Admin.Infrastructure.Services.Content;
using MrCMS.Web.Apps.Core.Areas.Admin.Models.Content;
using MrCMS.Web.Apps.Core.Entities.ContentBlocks;

namespace MrCMS.Web.Apps.Core.Areas.Admin.Services.Content;

public class QuoteAdminConfiguration : ContentBlockAdminConfigurationBase<Quote, UpdateQuoteAdminModel>
{
    public override UpdateQuoteAdminModel GetEditModel(Quote block)
    {
        return new UpdateQuoteAdminModel
        {
            CssClasses = block.CssClasses, QuoteFooter = block.QuoteFooter, QuoteText = block.QuoteText,
            BackgroundColour = block.BackgroundColour
        };
    }

    public override void UpdateBlock(Quote block, UpdateQuoteAdminModel editModel)
    {
        block.CssClasses = editModel.CssClasses;
        block.QuoteText = editModel.QuoteText;
        block.QuoteFooter = editModel.QuoteFooter;
        block.BackgroundColour = editModel.BackgroundColour;
    }
}