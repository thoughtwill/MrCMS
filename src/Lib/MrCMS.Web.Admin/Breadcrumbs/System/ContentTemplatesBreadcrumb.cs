using MrCMS.Web.Admin.Infrastructure.Breadcrumbs;

namespace MrCMS.Web.Admin.Breadcrumbs.System;

public class ContentTemplatesBreadcrumb : Breadcrumb<SystemBreadcrumb>
{
    public override string Name => "Content Templates";
    public override string Controller => "ContentTemplate";
    public override string Action => "Index";
    public override bool IsNav => true;
    public override decimal Order => 4;
}