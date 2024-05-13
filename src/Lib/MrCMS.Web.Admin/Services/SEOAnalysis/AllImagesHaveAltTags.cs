using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using MrCMS.Entities.Documents.Web;
using MrCMS.Services.Resources;
using MrCMS.Web.Admin.Models.SEOAnalysis;
using MrCMS.Web.Admin.Helpers;

namespace MrCMS.Web.Admin.Services.SEOAnalysis
{
    public class AllImagesHaveAltTags : BaseSEOAnalysisFacetProvider
    {
        private readonly IStringResourceProvider _stringResourceProvider;

        public AllImagesHaveAltTags(IStringResourceProvider stringResourceProvider)
        {
            _stringResourceProvider = stringResourceProvider;
        }

        public override async Task<IReadOnlyList<SEOAnalysisFacet>> GetFacets(Webpage webpage, HtmlNode document,
            string analysisTerm)
        {
            var images = document.GetElementsOfType("img").ToList();

            IEnumerable<HtmlNode> imagesWithoutAlt = images.FindAll(node => !node.Attributes.Contains("alt"));

            string hasNotGotAnAltTag =
                await _stringResourceProvider.GetValue("Admin SEO Analysis Has not got", configureOptions => configureOptions.SetDefaultValue(" has not got an alt tag"));
            return new List<SEOAnalysisFacet>
            {
                GetFacet(
                    await _stringResourceProvider.GetValue("Admin SEO Analysis Has not got alt tags", configureOptions => configureOptions.SetDefaultValue("All images have alt tags")),
                    !imagesWithoutAlt.Any()
                        ? SEOAnalysisStatus.Success
                        : SEOAnalysisStatus.Error,
                    imagesWithoutAlt.Select(node => node.GetAttributeValue("src", "") + hasNotGotAnAltTag).ToArray())
            };
        }
    }
}