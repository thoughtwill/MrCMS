using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MrCMS.ACL.Rules;
using MrCMS.Entities.People;
using MrCMS.Website.Auth;

namespace MrCMS.Services
{
    public class UserManager : UserManager<User>, IUserPasswordManager, IUserClaimManager, IUserLoginManager,
        IUserRoleManager, IUserEmailManager, IUserPhoneNumberManager, IUserLockoutManager,
        IUser2FAManager, IUserTokenManager, IGetUserFromClaims //, IUserLookup //IUserManager
    {
        private readonly IPerformACLCheck _performAclCheck;

        public UserManager(IUserStore<User> store, IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<User> passwordHasher, IEnumerable<IUserValidator<User>> userValidators,
            IEnumerable<IPasswordValidator<User>> passwordValidators, ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager> logger,
            IPerformACLCheck performAclCheck) : base(store,
            optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services,
            logger)
        {
            _performAclCheck = performAclCheck;
        }

        public async Task<IdentityResult> AssignRoles(User user, IList<string> roles)
        {
            var existingRoles = await GetRolesAsync(user);
            var toAdd = roles.Except(existingRoles, StringComparer.OrdinalIgnoreCase);
            var toRemove = existingRoles.Except(roles, StringComparer.OrdinalIgnoreCase);

            var result = await AddToRolesAsync(user, toAdd);
            if (!result.Succeeded)
                return result;

            result = await RemoveFromRolesAsync(user, toRemove);
            if (!result.Succeeded)
                return result;

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> SetPassword(User user, string password)
        {
            if (await HasPasswordAsync(user))
            {
                var result = await RemovePasswordAsync(user);
                if (!result.Succeeded)
                    return result;
            }

            return await AddPasswordAsync(user, password);
        }

        public override async Task<IList<Claim>> GetClaimsAsync(User user)
        {
            var claims = await base.GetClaimsAsync(user);

            // get the current user's impersonation claim
            var impersonationClaims = claims.Where(x =>
                x.Type is UserImpersonationService.UserImpersonationId
                    or UserImpersonationService.UserImpersonationName).ToList();

            // if the user is not impersonating, return the claims
            if (!impersonationClaims.Any())
                return claims;

            // check if they are still allowed to impersonate
            var roles = user.Roles.Select(x => x.Id).ToHashSet();
            var canImpersonate = user.IsAdmin || await _performAclCheck.CanAccessLogic(roles, typeof(UserACL), UserACL.Impersonate);

            // if that's still ok, return the claims
            if (canImpersonate) return claims;

            // if not, remove the impersonation claims
            await RemoveClaimsAsync(user, impersonationClaims);
            foreach (var impersonationClaim in impersonationClaims)
            {
                claims.Remove(impersonationClaim);
            }

            return claims;
        }

        public override string GetUserName(ClaimsPrincipal principal)
        {
            // get the standard user name
            var userName = base.GetUserName(principal);

            // get the current principal's impersonation claim
            var impersonationClaim =
                principal.Claims.FirstOrDefault(x => x.Type == UserImpersonationService.UserImpersonationName);

            // if the user is not impersonating, return the user name
            // otherwise return the name from the claim (we've checked it's valid in load)
            return impersonationClaim == null ? userName : impersonationClaim.Value;
        }

        public override string GetUserId(ClaimsPrincipal principal)
        {
            // get the standard user id
            var userId = base.GetUserId(principal);

            // get the current principal's impersonation claim
            var impersonationClaim =
                principal.Claims.FirstOrDefault(x => x.Type == UserImpersonationService.UserImpersonationId);

            // if they don't have one, return the standard user id
            // otherwise return the id from the claim (we've checked it's valid in load)
            return impersonationClaim == null ? userId : impersonationClaim.Value;
        }
    }
}