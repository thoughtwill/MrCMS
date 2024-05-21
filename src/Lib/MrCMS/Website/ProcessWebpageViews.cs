using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using MrCMS.Entities.Documents.Layout;
using MrCMS.Entities.Documents.Web;
using MrCMS.Helpers;
using MrCMS.Services;
using StackExchange.Profiling;

namespace MrCMS.Website
{
    public class ProcessWebpageViews : IProcessWebpageViews
    {
        public const string WidgetData = "widget-data";
        public const string LayoutFile = "layout-file";

        private readonly IGetCurrentLayout _getCurrentLayout;
        private readonly IGetWidgetDisplayInfo _getWidgetDisplayInfo;

        public ProcessWebpageViews(IGetCurrentLayout getCurrentLayout, IGetWidgetDisplayInfo getWidgetDisplayInfo)
        {
            _getCurrentLayout = getCurrentLayout;
            _getWidgetDisplayInfo = getWidgetDisplayInfo;
        }

        public async Task Process(ViewResult result, object model)
        {
            using (MiniProfiler.Current.Step("ProcessWebpageViews.Process"))
            {
                var webpage = result.Model as Webpage;
                //if webpage is null, its not a Mr CMS page, but we can use the default layout
                if (webpage == null)
                {
                    var layout = _getCurrentLayout.GetSiteDefault();
                    await SetLayoutViewData(result.ViewData, layout);
                    return;
                }

                if (string.IsNullOrWhiteSpace(result.ViewName))
                {
                    if (webpage.PageTemplate != null && !string.IsNullOrWhiteSpace(webpage.PageTemplate.PageTemplateName))
                        result.ViewName = webpage.PageTemplate.PageTemplateName;
                    else
                        result.ViewName = webpage.GetType().Name;
                }

                if (string.IsNullOrWhiteSpace(result.ViewData[LayoutFile]?.ToString()))
                {
                    Layout layout = _getCurrentLayout.Get(webpage);
                    if (layout != null)
                    {
                        await SetLayoutViewData(result.ViewData, layout);
                    }
                }
            }
        }

        public async Task ProcessForDefault(ViewDataDictionary viewData)
        {
            Layout layout = _getCurrentLayout.GetUserAccountLayout();
            if (layout != null)
            {
                await SetLayoutViewData(viewData, layout);
            }
        }

        private async Task SetLayoutViewData(ViewDataDictionary viewData, Layout layout)
        {
            viewData[LayoutFile] = layout.GetLayoutName();
            viewData[WidgetData] = await _getWidgetDisplayInfo.GetWidgets(layout);
        }
    }
}