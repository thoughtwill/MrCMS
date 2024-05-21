using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using MrCMS.ACL.Rules;
using MrCMS.Services;
using MrCMS.Website;
using MrCMS.Website.Auth;

namespace MrCMS.Web.Admin.Filters
{
    public class AdminAuthFilter : IAsyncAuthorizationFilter
    {
        // private readonly IAccessChecker _accessChecker;
        //
        // public AdminAuthFilter(IAccessChecker accessChecker)
        // {
        //     _accessChecker = accessChecker;
        // }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (!context.IsAdminRequest())
                return;

            // if the current user principal is null, then the user is not logged in
            var getCurrentUser = context.HttpContext.RequestServices.GetRequiredService<IGetCurrentClaimsPrincipal>();
            var user = await getCurrentUser.GetPrincipal();
            if (user == null)
            {
                context.Result = new ChallengeResult();
                return;
            }

            var accessChecker = context.HttpContext.RequestServices.GetRequiredService<IAccessChecker>();

            // check if the user is allowed to access the admin generally
            if (!await accessChecker.CanAccess<AdminAccessACL>(AdminAccessACL.Allowed, user))
            {
                context.Result = new ForbidResult();
                return;
            }

            // next check if there are 
            var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
            if (actionDescriptor == null)
                return;
            var aclAttribute = actionDescriptor.MethodInfo.GetCustomAttribute<AclAttribute>() ??
                               actionDescriptor.ControllerTypeInfo.GetCustomAttribute<AclAttribute>();

            // if there is no ACL attribute, then we can assume that the user has access
            // todo - check this
            if (aclAttribute == null)
                return;

            // otherwise, check the ACL using info from the attribute
            if (!await accessChecker.CanAccess(aclAttribute.Type, aclAttribute.Operation, user))
                context.Result = new ForbidResult();
        }
    }
}