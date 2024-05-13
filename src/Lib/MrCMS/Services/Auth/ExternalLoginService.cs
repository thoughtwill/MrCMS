using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using MrCMS.Entities.People;
using MrCMS.Services.Resources;

namespace MrCMS.Services.Auth
{
    public class ExternalLoginService : IExternalLoginService
    {
        private readonly ISignInManager _signInManager;
        private readonly IStringResourceProvider _stringLocalizer;
        private readonly IUserLoginManager _userLoginManager;
        private readonly IUserClaimManager _userClaimManager;

        private readonly IUserLookup _userLookup;

        private readonly IUserManagementService _userManagementService;

        public ExternalLoginService(ISignInManager signInManager,
            IStringResourceProvider stringLocalizer, IUserLoginManager userLoginManager,
            IUserClaimManager userClaimManager,
            IUserLookup userLookup,
            IUserManagementService userManagementService)
        {
            _signInManager = signInManager;
            _stringLocalizer = stringLocalizer;
            _userLoginManager = userLoginManager;
            _userClaimManager = userClaimManager;
            _userLookup = userLookup;
            _userManagementService = userManagementService;
        }

        public AuthenticationProperties GetExternalConfigurationProperties(string provider, string redirectUrl)
        {
            return _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        }

        public async Task<ExternalLoginCallbackResult> HandleExternalLoginCallback(string returnUrl, string remoteError)
        {
            if (remoteError != null)
            {
                return new ExternalLoginCallbackResult
                {
                    Error =await _stringLocalizer.GetValue("Error from external provider: {remoteError}",configureOptions: options => options.AddReplacement("remoteError",remoteError) )
                };
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return new ExternalLoginCallbackResult { Error = await _stringLocalizer.GetValue("An error has occurred") };
            }

            // Sign in the user with this external login provider if the user already has a login.
            return new ExternalLoginCallbackResult
            {
                Success = true,
                Result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false),
                LoginInfo = info
            };
        }

        public async Task<User> CreateAccountFromExternalLogin(ExternalLoginConfirmationViewModel model)
        {
            // Get the information about the user from the external login provider
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return null;
            }
            var user = new User { Email = model.Email };
            await _userManagementService.AddUser(user);
            var result = await _userLoginManager.AddLoginAsync(user, info);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user);
                //_logger.LogInformation(6, "User created an account using {Name} provider.", info.LoginProvider);
                return user;
            }

            return user;

        }

        public string GetEmail(AuthenticateResult authenticateResult)
        {
            var principal = authenticateResult?.Principal;
            return GetEmail( principal);
        }

        private static string GetEmail(ClaimsPrincipal principal)
        {
            if (principal != null)
            {
                IEnumerable<Claim> claims = principal.Claims;
                Claim emailClaim = claims?.FirstOrDefault(claim => claim.Type == ClaimTypes.Email);
                if (emailClaim != null)
                    return emailClaim.Value;
            }

            return null;
        }

        public async Task<User> GetUserToLogin(ExternalLoginInfo loginInfo)
        {
            var email = GetEmail(loginInfo.Principal);
            var user = await _userLoginManager.FindByLoginAsync(loginInfo.LoginProvider, loginInfo.ProviderKey);
            if (user != null)
                return user;
            user = await _userLookup.GetUserByEmail(email);
            if (user == null)
            {
                user = new User { Email = email, IsActive = true };
                await _userManagementService.AddUser(user);
            }
            await _userLoginManager.AddLoginAsync(user, loginInfo);
            return user;
        }

        public async Task UpdateClaimsAsync(User user, IEnumerable<Claim> claims)
        {
            var existingClaims = (await _userClaimManager.GetClaimsAsync(user)).ToList();
            var list = claims as IList<Claim> ?? claims.ToList();
            var newClaims =
                list.Where(claim => existingClaims.All(c => c.Type != claim.Type));
            var updatedClaims =
                list.Where(claim => existingClaims.Any(c => c.Type == claim.Type && c.Value != claim.Value));
            foreach (var newClaim in newClaims)
                await _userClaimManager.AddClaimAsync(user, newClaim);
            foreach (var updatedClaim in updatedClaims)
            {
                var existing = existingClaims.First(c => c.Type == updatedClaim.Type);
                await _userClaimManager.RemoveClaimAsync(user, existing);
                await _userClaimManager.AddClaimAsync(user, updatedClaim);
            }
        }
    }
}