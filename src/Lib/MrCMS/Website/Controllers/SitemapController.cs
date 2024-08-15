using Microsoft.AspNetCore.Mvc;
using MrCMS.Services;
using MrCMS.Services.Sitemaps;

namespace MrCMS.Website.Controllers;

public class SitemapController : MrCMSUIController
{
    private const string SitemapUrl = "sitemap.xml";
    private readonly IGetSitemapPath _getSitemapPath;
    private readonly ICurrentSiteLocator _siteLocator;

    public SitemapController(IGetSitemapPath getSitemapPath, ICurrentSiteLocator siteLocator)
    {
        _getSitemapPath = getSitemapPath;
        _siteLocator = siteLocator;
    }

    [Route(SitemapUrl)]
    public ActionResult Show()
    {
        var site = _siteLocator.GetCurrentSite();
        if (!_getSitemapPath.FileExists(site))
            return new EmptyResult();

        return PhysicalFile(_getSitemapPath.GetAbsolutePath(site), "application/xml");
    }

}