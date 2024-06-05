using MrCMS.Web.Admin.Infrastructure.Breadcrumbs;

namespace MrCMS.Web.Admin.Breadcrumbs.System;

public class RecurringJobsBreadcrumb : Breadcrumb<SystemBreadcrumb>
{
    public override string Name => "Recurring Jobs";
    public override decimal Order => 10;
    public override string Controller => "RecurringJob";
    public override string Action => "Index";
    public override bool IsNav => true;
}