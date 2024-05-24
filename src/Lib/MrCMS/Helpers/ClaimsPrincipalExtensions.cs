using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using MrCMS.Entities.People;
using MrCMS.Services;

namespace MrCMS.Helpers;

public static class ClaimsPrincipalExtensions
{
    public static int? GetUserId(this ClaimsPrincipal principal)
    {
        // try and get the value from the NameIdentifier claim first
        var value = principal?.FindFirstValue(ClaimTypes.NameIdentifier);

        // if that's not available, we're not logged in
        if (value == null)
        {
            return null;
        }

        // otherwise, try and parse the value as an int
        return int.TryParse(value, out var id) ? id : null;
    }


    public static IEnumerable<int> GetRoleIds(this ClaimsPrincipal principal)
    {
        return principal?.FindAll(MrCMSKnownClaimTypes.RoleId).Select(x => int.TryParse(x.Value, out var id) ? id : 0)
            .Where(x => x > 0).ToArray() ?? [];
    }

    public static string GetEmail(this ClaimsPrincipal principal)
    {
        return principal?.FindFirstValue(MrCMSKnownClaimTypes.Email);
    }

    public static string GetFirstName(this ClaimsPrincipal principal)
    {
        return principal?.FindFirstValue(MrCMSKnownClaimTypes.FirstName);
    }

    public static string GetLastName(this ClaimsPrincipal principal)
    {
        return principal?.FindFirstValue(MrCMSKnownClaimTypes.LastName);
    }

    public static string GetFullName(this ClaimsPrincipal principal)
    {
        return principal?.FindFirstValue(MrCMSKnownClaimTypes.Name);
    }

    public static bool IsInRole(this ClaimsPrincipal principal, int id)
    {
        var roles = GetRoleIds(principal);
        return roles.Contains(id);
    }


    public static bool IsInAnyRole(this ClaimsPrincipal principal, params int[] roles)
    {
        return roles.Any(principal.IsInRole);
    }

    public static bool IsInAnyRole(this ClaimsPrincipal principal, IEnumerable<int> roles)
    {
        return roles.Any(principal.IsInRole);
    }

    public static bool IsInAllRoles(this ClaimsPrincipal principal, params int[] roles)
    {
        return roles.All(principal.IsInRole);
    }

    public static bool IsInAllRoles(this ClaimsPrincipal principal, IEnumerable<int> roles)
    {
        return roles.All(principal.IsInRole);
    }

    public static bool IsAdmin(this ClaimsPrincipal principal)
    {
        return principal?.FindFirstValue(MrCMSKnownClaimTypes.IsAdmin) == "true";
    }

    public static bool DisableNotifications(this ClaimsPrincipal principal)
    {
        return principal?.FindFirstValue(MrCMSKnownClaimTypes.DisableNotifications) == "true";
    }

    public static string GetAvatarUrl(this ClaimsPrincipal principal)
    {
        return principal?.FindFirstValue(MrCMSKnownClaimTypes.Avatar);
    }

    public static Guid? GetUserGuid(this ClaimsPrincipal principal)
    {
        var value = principal?.FindFirstValue(MrCMSKnownClaimTypes.UserGuid);

        return Guid.TryParse(value, out var guid) ? guid : null;
    }

    public static string GetUserCulture(this ClaimsPrincipal principal)
    {
        return principal?.FindFirstValue(MrCMSKnownClaimTypes.UserCulture);
    }
}