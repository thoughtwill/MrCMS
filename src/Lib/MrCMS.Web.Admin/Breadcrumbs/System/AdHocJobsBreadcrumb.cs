using MrCMS.Web.Admin.Infrastructure.Breadcrumbs;

namespace MrCMS.Web.Admin.Breadcrumbs.System;

public class AdHocJobsBreadcrumb : Breadcrumb<SystemBreadcrumb>
{
    public override string Name => "Ad-Hoc Jobs";
    public override decimal Order => 11;
    public override string Controller => "AdHocJob";
    public override string Action => "Index";
    public override bool IsNav => true;
}