using MrCMS.Web.Admin.Infrastructure.Breadcrumbs;

namespace MrCMS.Web.Admin.Breadcrumbs.System;

public class PromptTemplatesBreadcrumb : Breadcrumb<SystemBreadcrumb>
{
    public override string Name => "Prompt Templates";
    public override string Controller => "Prompt";
    public override string Action => "Index";
    public override bool IsNav => true;
    public override decimal Order => 4;
}