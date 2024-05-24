using System.Linq;
using System.Security.Claims;
using MrCMS.Helpers;

namespace MrCMS.Website.Auth
{
    public class CheckStandardAccessLogic : ICheckStandardAccessLogic
    {
        public StandardLogicCheckResult Check(ClaimsPrincipal user)
        {
            // must be logged in
            if (user == null)
                return new StandardLogicCheckResult {CanAccess = false};

            return GetResult(user);
        }

        private StandardLogicCheckResult GetResult(ClaimsPrincipal user)
        {
            // if they're an admin they're always allowed
            if (user.IsAdmin())
                return new StandardLogicCheckResult {CanAccess = true};

            // if the user has no roles, they cannot have any acl access granted
            var roles = user.GetRoleIds();
            if (!roles.Any())
                return new StandardLogicCheckResult {CanAccess = false};

            return new StandardLogicCheckResult {Roles = roles.ToHashSet()};
        }
    }}