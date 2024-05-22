using System.Security.Claims;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using MrCMS.Entities.People;

namespace MrCMS.Services;

public class GetCurrentClaimsPrincipal : IGetCurrentClaimsPrincipal
{
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IUserImpersonationService _userImpersonationService;
    private readonly SignInManager<User> _signInManager;

    public GetCurrentClaimsPrincipal(IHttpContextAccessor contextAccessor,
        IUserImpersonationService userImpersonationService,
        SignInManager<User> signInManager)
    {
        _contextAccessor = contextAccessor;
        _userImpersonationService = userImpersonationService;
        _signInManager = signInManager;
    }

    [ItemCanBeNull]
    public async Task<ClaimsPrincipal> GetPrincipal()
    {
        return await GetImpersonatedClaimsPrincipal() ?? await GetLoggedInClaimsPrincipal();
    }

    [ItemCanBeNull]
    public Task<ClaimsPrincipal> GetLoggedInClaimsPrincipal()
    {
        // we only want to return the user if they are logged in
        var user = _contextAccessor.HttpContext?.User;
        return user?.Identity?.IsAuthenticated == true ? Task.FromResult(user) : Task.FromResult((ClaimsPrincipal) null);
    }

    [ItemCanBeNull]
    public async Task<ClaimsPrincipal> GetImpersonatedClaimsPrincipal()
    {
        // if the current logged in principal is null, we're done
        var loggedInPrincipal = await GetLoggedInClaimsPrincipal();
        if (loggedInPrincipal == null)
            return null;

        // otherwise we need to check if they have impersonated a user
        var impersonatedUser = await _userImpersonationService.GetCurrentlyImpersonatedUser(loggedInPrincipal);

        // if they haven't, we're done
        if (impersonatedUser == null)
            return null;

        // otherwise we need to use the SignInManager to build a new ClaimsPrincipal
        return await _signInManager.CreateUserPrincipalAsync(impersonatedUser);
    }
}