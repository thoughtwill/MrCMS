using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MrCMS.Entities.Documents.Layout;
using MrCMS.Entities.Widget;
using MrCMS.Helpers;
using MrCMS.Settings;
using MrCMS.Web.Apps.Core.Widgets;
using NHibernate;

namespace MrCMS.Web.Apps.Core.Services.Installation
{
    public class SetupCoreLayouts : ISetupCoreLayouts
    {
        private readonly ISession _session;
        private readonly IConfigurationProvider _configurationProvider;

        public SetupCoreLayouts(ISession session, IConfigurationProvider configurationProvider)
        {
            _session = session;
            _configurationProvider = configurationProvider;
        }

        public async Task Setup()
        {
            Layout baseLayout = null;
            await _session.TransactAsync(async session =>
            {
                baseLayout = new Layout
                {
                    Name = "Base Layout",
                    Path = "_BaseLayout",
                    LayoutAreas = new List<LayoutArea>()
                };

                await session.SaveAsync(baseLayout);
            });

            var siteSettings = _configurationProvider.GetSiteSettings<SiteSettings>();
            siteSettings.DefaultLayoutId = baseLayout.Id;
            await _configurationProvider.SaveSettings(siteSettings);

            await _session.TransactAsync(async session =>
            {
                List<LayoutArea> layoutAreas = GetDefaultAreas(baseLayout);
                foreach (var area in layoutAreas)
                {
                    await session.SaveAsync(area);
                }

                var layoutTwoColumn = new Layout
                {
                    Parent = baseLayout,
                    Name = "Two Column"
                };

                await session.SaveAsync(layoutTwoColumn);

                var layoutAreasTwoColumn = new List<LayoutArea>
                {
                    new LayoutArea
                    {
                        AreaName = "Right Column",
                        Layout = layoutTwoColumn
                    }
                };
                foreach (LayoutArea layoutArea in layoutAreasTwoColumn)
                    await session.SaveAsync(layoutArea);

                var widgets = AddWidgets(layoutAreas);
                foreach (var widget in widgets)
                {
                    await session.SaveAsync(widget);
                }
            });
        }

        private List<LayoutArea> GetDefaultAreas(Layout baseLayout)
        {
            var layoutAreas = new List<LayoutArea>
            {
                new LayoutArea
                {
                    AreaName = "Main Navigation",
                    CreatedOn = DateTime.UtcNow,
                    Layout = baseLayout,
                },
                new LayoutArea
                {
                    AreaName = "Header Left",
                    CreatedOn = DateTime.UtcNow,
                    Layout = baseLayout,
                },
                new LayoutArea
                {
                    AreaName = "Header Right",
                    CreatedOn = DateTime.UtcNow,
                    Layout = baseLayout,
                },
                new LayoutArea
                {
                    AreaName = "Before Content",
                    CreatedOn = DateTime.UtcNow,
                    Layout = baseLayout,
                },
                new LayoutArea
                {
                    AreaName = "After Content",
                    CreatedOn = DateTime.UtcNow,
                    Layout = baseLayout,
                },
                new LayoutArea
                {
                    AreaName = "Footer",
                    CreatedOn = DateTime.UtcNow,
                    Layout = baseLayout,
                }
            };
            return layoutAreas;
        }

        private IEnumerable<Widget> AddWidgets(List<LayoutArea> layoutAreas)
        {
            yield return new Navigation
            {
                Name = "Navigation",
                IncludeChildren = true,
                LayoutArea = layoutAreas.Single(x => x.AreaName == "Main Navigation")
            };


            yield return new TextWidget
            {
                Name = "Footer text",
                Text = $"<p>&copy; Mr CMS {DateTime.UtcNow.Year}</p>",
                LayoutArea = layoutAreas.Single(x => x.AreaName == "Footer")
            };
        }
    }
}