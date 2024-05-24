using System.Linq;
using System.Threading.Tasks;
using MrCMS.Entities.Documents.Web;
using MrCMS.Entities.People;
using MrCMS.Helpers;

namespace MrCMS.Services
{
    public class UserUIPermissionsService : IUserUIPermissionsService
    {
        private readonly IGetCurrentClaimsPrincipal _getCurrentClaimsPrincipal;

        public UserUIPermissionsService(IGetCurrentClaimsPrincipal getCurrentClaimsPrincipal)
        {
            _getCurrentClaimsPrincipal = getCurrentClaimsPrincipal;
        }

        public async Task<PageAccessPermission> IsCurrentUserAllowed(Webpage webpage)
        {
            if (!webpage.HasCustomPermissions) return PageAccessPermission.Allowed;

            var user = await _getCurrentClaimsPrincipal.GetPrincipal();
            if (user != null && user.IsAdmin()) return PageAccessPermission.Allowed;

            var webpageFrontEndAllowedRoles = webpage.FrontEndAllowedRoles.Select(x => x.Id).ToList();
            if (!webpageFrontEndAllowedRoles.Any()) return PageAccessPermission.Allowed;
            if (webpageFrontEndAllowedRoles.Any() && user == null) return PageAccessPermission.Unauthorized;

            return user.IsInAnyRole(webpageFrontEndAllowedRoles)
                ? PageAccessPermission.Allowed
                : PageAccessPermission.Forbidden;
        }
    }
}