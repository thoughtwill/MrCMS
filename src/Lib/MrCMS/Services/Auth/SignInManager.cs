using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MrCMS.Entities.People;
using MrCMS.Helpers;

namespace MrCMS.Services.Auth
{
    public class SignInManager : SignInManager<User>, ISignInManager
    {
        private readonly IEventContext _context;
        private readonly IGetCurrentUserGuid _getCurrentUserGuid;

        public SignInManager(UserManager<User> userManager, IHttpContextAccessor contextAccessor,
            IUserClaimsPrincipalFactory<User> claimsFactory, IOptions<IdentityOptions> optionsAccessor,
            ILogger<SignInManager<User>> logger, IAuthenticationSchemeProvider schemes,
            IUserConfirmation<User> userConfirmation, IEventContext context,
            IGetCurrentUserGuid getCurrentUserGuid)
            : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, userConfirmation)
        {
            _context = context;
            _getCurrentUserGuid = getCurrentUserGuid;
        }


        public override async Task SignInWithClaimsAsync(User user, AuthenticationProperties authenticationProperties,
            IEnumerable<Claim> additionalClaims)
        {
            var previousSession = await _getCurrentUserGuid.Get();
            await base.SignInWithClaimsAsync(user, authenticationProperties, additionalClaims);
            await _context.Publish<IOnUserLoggedIn, UserLoggedInEventArgs>(
                new UserLoggedInEventArgs(user.Unproxy(), previousSession));
        }

        public override async Task SignOutAsync()
        {
            var currentUser = Context.User;
            await base.SignOutAsync();
            await _context.Publish<IOnLoggedOut, LoggedOutEventArgs>(new LoggedOutEventArgs(currentUser));
        }
        
        public override async Task<User> ValidateSecurityStampAsync(ClaimsPrincipal principal)
        {
            if (principal == null)
                return default;
            var loggedInUserId = principal.GetUserId();
            var user = await this.UserManager.FindByIdAsync(loggedInUserId.ToString());
            if (await ValidateSecurityStampAsync(user,
                    principal.FindFirstValue(this.Options.ClaimsIdentity.SecurityStampClaimType)))
                return user;
            return default;
        }
    }
}