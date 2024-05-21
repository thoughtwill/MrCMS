using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MrCMS.Settings
{
    public interface ISiteSettingsOptionGenerator
    {
        List<SelectListItem> GetTopLevelPageOptions(int pageId);
        List<SelectListItem> GetMediaCategoryOptions(int? categoryId);
        List<SelectListItem> GetLayoutOptions(int? selectedLayoutId);
        List<SelectListItem> GetThemeNames(string themeName);
        List<SelectListItem> GetUiCultures(string uiCulture);
        List<SelectListItem> GetTimeZones(string timeZone);
        List<SelectListItem> GetFormRendererOptions(FormRenderingType defaultFormRendererType);
        List<SelectListItem> GetFormLabelRendererOptions(FormLabelRenderingType defaultFormLabelRendererType);
        List<SelectListItem> GetCacheExpiryTypeOptions();
    }
}