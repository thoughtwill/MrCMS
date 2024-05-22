using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using MrCMS.ACL.Rules;
using MrCMS.Entities.People;
using MrCMS.Helpers;
using MrCMS.Models;
using MrCMS.Services.Auth;
using MrCMS.Website.Auth;

namespace MrCMS.Services;

public class UserImpersonationService : IUserImpersonationService, IOnLoggedOut
{
    private readonly IAccessChecker _accessChecker;

    private readonly IUserClaimManager _userClaimManager;
    private readonly IUserStore<User> _userStore;
    public const string UserImpersonationId = "IsCurrentlyImpersonating.Id";
    public const string UserImpersonationName = "IsCurrentlyImpersonating.Name";
    public static readonly string[] ImpersonationKeys = {UserImpersonationId, UserImpersonationName};

    public UserImpersonationService(IAccessChecker accessChecker, IUserClaimManager userClaimManager,
        IUserStore<User> userStore)
    {
        _accessChecker = accessChecker;
        _userClaimManager = userClaimManager;
        _userStore = userStore;
    }

    public async Task<UserImpersonationResult> Impersonate(ClaimsPrincipal currentPrincipal, User userToImpersonate, CancellationToken cancellationToken = default)
    {
        if (userToImpersonate == null)
            return UserImpersonationResult.ErrorResult("User not found");


        // if (currentPrincipal.Identity is not ClaimsIdentity identity)
        //     return new UserImpersonationResult {Error = "Must be logged in to impersonate"};

        var currentUserId = currentPrincipal.GetUserId();
        if (currentUserId == null)
            return UserImpersonationResult.ErrorResult("Must be logged in to impersonate");

        var canAccess = await _accessChecker.CanAccess<UserACL>(UserACL.Impersonate, currentPrincipal);
        if (!canAccess)
        {
            return UserImpersonationResult.ErrorResult("Only admins can impersonate another user");
        }

        var currentUser = await _userStore.FindByIdAsync(currentUserId.ToString(), cancellationToken);
        if (currentUser == null)
            return UserImpersonationResult.ErrorResult("Can only impersonate when logged in");

        if (userToImpersonate.Id == currentUser.Id)
            return UserImpersonationResult.ErrorResult("Cannot impersonate self");

        if (userToImpersonate.IsAdmin)
            return UserImpersonationResult.ErrorResult("Cannot impersonate an admin");

        var idClaim = new Claim(UserImpersonationId, userToImpersonate.Id.ToString());
        var nameClaim = new Claim(UserImpersonationName, userToImpersonate.Email);

        // Add claims to the current user.
        await _userClaimManager.AddClaimsAsync(currentUser, new List<Claim> { idClaim, nameClaim });

        return UserImpersonationResult.SuccessResult(currentUser);
    }

    // private async Task<User> GetCurrentUserFromContext(HttpContext httpContext)
    // {
    //     var currentUser = await _userStore.FindByNameAsync(httpContext.User.Identity?.Name, default);
    //     return currentUser;
    // }

    public async Task<User> GetCurrentlyImpersonatedUser(ClaimsPrincipal principal)
    {
        // get the impersonation claim for the user
        var (id, name) = GetImpersonationClaims(principal);

        // if they don't have one, return null
        if (id == null)
            return null;

        // get the user id from the claim
        var impersonatedUserId = id.Value;

        // get the user by that id
        return await _userStore.FindByIdAsync(impersonatedUserId, default);
    }


    public async Task<User> CancelImpersonation(ClaimsPrincipal principal)
    {
        // var 
        // var currentUser = await GetCurrentUserFromContext(httpContext);
        // if (currentUser == null)
        //     return;
        var identity = principal.Identity as ClaimsIdentity;
        if (identity == null)
            return null;

        var (id, name) = GetImpersonationClaims(principal);
        var currentUser = await _userStore.FindByIdAsync(principal.GetUserId()?.ToString() ?? string.Empty, default);
        if (currentUser == null)
            return null;

        // we want to remove the one with the user impersonation key if it exists 
        if (id != null)
        {
            identity.TryRemoveClaim(id);
            await _userClaimManager.RemoveClaimAsync(currentUser, id);
        }

        if (name != null)
        {
            identity.TryRemoveClaim(name);
            await _userClaimManager.RemoveClaimAsync(currentUser, name);
        }
        
        return currentUser;
    }

    private static (Claim id, Claim name) GetImpersonationClaims(ClaimsPrincipal user)
    {
        var existingClaims = user.Claims.ToList();
        var id = existingClaims.FirstOrDefault(x => x.Type == UserImpersonationId);
        var name = existingClaims.FirstOrDefault(x => x.Type == UserImpersonationName);
        return (id, name);
    }

    public async Task Execute(LoggedOutEventArgs args)
    {
        await CancelImpersonation(args.User);
    }
}