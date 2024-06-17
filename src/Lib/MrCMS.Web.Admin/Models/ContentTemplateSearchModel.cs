namespace MrCMS.Web.Admin.Models;

public class ContentTemplateSearchModel
{
    public ContentTemplateSearchModel()
    {
        Page = 1;
    }
    
    public int Page { get; set; }
    public string Name { get; set; }
}