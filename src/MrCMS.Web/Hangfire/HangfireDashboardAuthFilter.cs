using Hangfire.Dashboard;
using MrCMS.Entities.People;

namespace MrCMS.Web.Hangfire;

public class HangfireDashboardAuthFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        return context.GetHttpContext().User.IsInRole(UserRole.Administrator);
    }
}