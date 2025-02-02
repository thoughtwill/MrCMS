using MrCMS.Web.Admin.Infrastructure.Breadcrumbs;

namespace MrCMS.Web.Admin.Breadcrumbs.System.Settings
{
    public class AiSettingsBreadcrumb : Breadcrumb<SettingsBreadcrumb>
    {
        public override string Name => "AI Settings";
        public override string Controller => "AiSettings";
        public override string Action => "Index";

        public override bool IsNav => true;
        public override decimal Order => 1;
    }
}