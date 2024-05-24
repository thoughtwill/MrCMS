using System;
using Microsoft.AspNetCore.Identity;
using MrCMS.Entities.People;
using MrCMS.Helpers;
using NHibernate;
using NHibernate.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MrCMS.Services
{
    public class UserStore :
        IUserLockoutStore<User>,
        IUserClaimStore<User>,
        IUserEmailStore<User>,
        IUserSecurityStampStore<User>,
        IUserRoleStore<User>,
        IUserPasswordStore<User>,
        IQueryableUserStore<User>,
        IUserLoginStore<User>,
        IUserTwoFactorStore<User>,
        IUserTwoFactorRecoveryCodeStore<User>,
        IUserPhoneNumberStore<User>,
        IUserAuthenticatorKeyStore<User>,
        IUserAuthenticationTokenStore<User>
    {
        private readonly ISession _session;


        public UserStore(ISession session)
        {
            _session = session;
        }

        public void Dispose()
        {
        }

        public Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user?.Id.ToString());
        }

        public Task<string> GetUserNameAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user?.Email);
        }

        public Task SetUserNameAsync(User user, string userName, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task<string> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user?.Email);
        }

        public Task SetNormalizedUserNameAsync(User user, string normalizedName, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public async Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken)
        {
            await _session.TransactAsync((session, token) => session.SaveAsync(user, token), cancellationToken);
            // _session.Transact(session => session.Save(user));
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken)
        {
            // _session.Transact(session => session.Update(user));
            await _session.TransactAsync((session, token) => session.UpdateAsync(user, token), cancellationToken);
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken)
        {
            // _session.Transact(session => session.Delete(user));
            await _session.TransactAsync((session, token) => session.DeleteAsync(user, token), cancellationToken);
            return IdentityResult.Success;
        }

        public async Task<User> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            return int.TryParse(userId, out var id)
                ? await _session.GetAsync<User>(id, cancellationToken)
                : null;
        }

        public Task<User> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            return _session.Query<User>().WithOptions(x => x.SetCacheable(true))
                .FirstOrDefaultAsync(x => x.Email == normalizedUserName, cancellationToken);
        }

        public async Task<IList<Claim>> GetClaimsAsync(User user, CancellationToken cancellationToken)
        {
            var userClaims = await GetUserClaimsAsync(user, cancellationToken);

            var claims = userClaims.Select(claim => new Claim(claim.Claim, claim.Value, null, claim.Issuer)).ToList();

            // add the role ids as claims
            var roles = user.Roles.ToList();
            claims.AddRange(roles.Select(role => new Claim(MrCMSKnownClaimTypes.RoleId, role.Id.ToString())));

            // add email claim
            if (!string.IsNullOrWhiteSpace(user.Email))
                claims.Add(new Claim(MrCMSKnownClaimTypes.Email, user.Email));
            
            // add name claims
            if (!string.IsNullOrWhiteSpace(user.Name))
                claims.Add(new Claim(MrCMSKnownClaimTypes.Name, user.Name));
            if (!string.IsNullOrWhiteSpace(user.LastName))
                claims.Add(new Claim(MrCMSKnownClaimTypes.LastName, user.LastName));
            if (!string.IsNullOrWhiteSpace(user.FirstName))
                claims.Add(new Claim(MrCMSKnownClaimTypes.FirstName, user.FirstName));


            // add avatar claim
            if (!string.IsNullOrWhiteSpace(user.AvatarImage))
                claims.Add(new Claim(MrCMSKnownClaimTypes.Avatar, user.AvatarImage));

            // add disable notifications claim
            claims.Add(new Claim(MrCMSKnownClaimTypes.DisableNotifications,
                user.DisableNotifications ? "true" : "false"));

            // add the user guid as a claim
            claims.Add(new Claim(MrCMSKnownClaimTypes.UserGuid, user.Guid.ToString()));

            // if the user has a custom culture, add it as a claim
            if (!string.IsNullOrWhiteSpace(user.UICulture))
                claims.Add(new Claim(MrCMSKnownClaimTypes.UserCulture, user.UICulture));

            if (user.IsAdmin)
                claims.Add(new Claim(MrCMSKnownClaimTypes.IsAdmin, "true"));

            return claims;
        }

        private async Task<List<UserClaim>> GetUserClaimsAsync(User user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                return [];
            }

            return await _session.Query<UserClaim>().Where(x => x.User.Id == user.Id)
                .WithOptions(options => options.SetCacheable(true))
                .ToListAsync(cancellationToken);
        }

        public Task AddClaimsAsync(User user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            var userClaims = claims.Select(claim => MapUserClaim(claim, user));
            return _session.TransactAsync(async (session, token) =>
            {
                foreach (var userClaim in userClaims)
                {
                    await session.SaveAsync(userClaim, token);
                    user.UserClaims.Add(userClaim);
                }

                await session.UpdateAsync(user, token);
            }, cancellationToken);
        }

        private static UserClaim MapUserClaim(Claim claim, User user)
        {
            return new()
            {
                Claim = claim.Type,
                Value = claim.Value,
                Issuer = claim.Issuer,
                User = user
            };
        }

        public async Task ReplaceClaimAsync(User user, Claim claim, Claim newClaim, CancellationToken cancellationToken)
        {
            var existingClaims = await GetUserClaimsAsync(user, cancellationToken);
            var existingUserClaim = existingClaims.FirstOrDefault(x => x.Claim == claim.Type);
            var newUserClaim = MapUserClaim(newClaim, user);

            await _session.TransactAsync(async (session, token) =>
            {
                await session.DeleteAsync(existingUserClaim, token);
                user.UserClaims.Remove(existingUserClaim);

                await session.SaveAsync(newUserClaim, token);
                user.UserClaims.Add(newUserClaim);

                await session.UpdateAsync(user, token);
            }, cancellationToken);
        }

        public async Task RemoveClaimsAsync(User user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            var existingClaims = await GetUserClaimsAsync(user, cancellationToken);
            await _session.TransactAsync(async (session, token) =>
            {
                foreach (var claim in claims)
                {
                    var existingUserClaim = existingClaims.FirstOrDefault(x => x.Claim == claim.Type);
                    if (existingUserClaim == null)
                    {
                        continue;
                    }

                    await session.DeleteAsync(existingUserClaim, token);
                    user.UserClaims.Remove(existingUserClaim);
                }

                await session.UpdateAsync(user, token);
            }, cancellationToken);
        }

        public async Task<IList<User>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
        {
            return await _session.Query<UserClaim>()
                .Where(x => x.Claim == claim.Type && x.Value == claim.Value)
                .WithOptions(options => options.SetCacheable(true))
                .Fetch(x => x.User)
                .Select(x => x.User)
                .ToListAsync(cancellationToken);
        }

        public async Task SetEmailAsync(User user, string email, CancellationToken cancellationToken)
        {
            user.Email = email;
            await UpdateAsync(user, cancellationToken);
        }

        public Task<string> GetEmailAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user?.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.EmailConfirmed);
        }

        public Task SetEmailConfirmedAsync(User user, bool confirmed, CancellationToken cancellationToken)
        {
            user.EmailConfirmed = confirmed;
            return Task.CompletedTask;
        }

        public async Task<User> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            return await _session.Query<User>().WithOptions(x => x.SetCacheable(true))
                .FirstOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken);
        }

        public Task<string> GetNormalizedEmailAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user?.Email);
        }

        public Task SetNormalizedEmailAsync(User user, string normalizedEmail, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public async Task AddToRoleAsync(User user, string roleName, CancellationToken cancellationToken)
        {
            var role = await GetRoleByName(roleName, cancellationToken);
            if (role != null)
            {
                user.Roles.Add(role);

                await _session.TransactAsync((session, token) => session.UpdateAsync(user, token), cancellationToken);
            }
        }

        private async Task<UserRole> GetRoleByName(string roleName, CancellationToken cancellationToken)
        {
            if (roleName == null)
                return null;
            var role = await _session.Query<UserRole>().WithOptions(x => x.SetCacheable(true))
                .FirstOrDefaultAsync(x => x.Name.ToUpper() == roleName.ToUpper(), cancellationToken);
            return role;
        }

        public async Task RemoveFromRoleAsync(User user, string roleName, CancellationToken cancellationToken)
        {
            var role = await GetRoleByName(roleName, cancellationToken);
            if (role != null)
            {
                if (user.Roles.Contains(role))
                {
                    user.Roles.Remove(role);
                }

                await _session.TransactAsync((session, token) => session.UpdateAsync(user, token), cancellationToken);
            }
        }

        public Task<IList<string>> GetRolesAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult<IList<string>>(user.Roles.Select(x => x.Name).ToList());
        }

        public async Task<bool> IsInRoleAsync(User user, string roleName, CancellationToken cancellationToken)
        {
            var role = await GetRoleByName(roleName, cancellationToken);
            return user.Roles.Contains(role);
        }

        public async Task<IList<User>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            var role = await GetRoleByName(roleName, cancellationToken);
            return role.Users.ToList();
        }

        public IQueryable<User> Users => _session.Query<User>().WithOptions(x => x.SetCacheable(true));

        public async Task AddLoginAsync(User user, UserLoginInfo login, CancellationToken cancellationToken)
        {
            var userLogin = new UserLogin
            {
                LoginProvider = login.LoginProvider,
                ProviderKey = login.ProviderKey,
                DisplayName = login.ProviderDisplayName,
                User = user
            };
            user.UserLogins.Add(userLogin);
            await _session.TransactAsync(async (session, token) =>
            {
                await session.SaveAsync(userLogin, token);
                await session.UpdateAsync(user, token);
            }, cancellationToken);
        }

        public async Task RemoveLoginAsync(User user, string loginProvider, string providerKey,
            CancellationToken cancellationToken)
        {
            UserLogin userLogin =
                user.UserLogins.FirstOrDefault(l => l.ProviderKey == providerKey && l.LoginProvider == loginProvider);
            if (userLogin != null)
            {
                user.UserLogins.Remove(userLogin);
                await _session.TransactAsync(async (session, token) =>
                {
                    await session.DeleteAsync(userLogin, token);
                    await session.UpdateAsync(user, token);
                }, cancellationToken);
            }
        }

        public Task<IList<UserLoginInfo>> GetLoginsAsync(User user, CancellationToken cancellationToken)
        {
            IList<UserLoginInfo> list =
                user.UserLogins.Select(login =>
                        new UserLoginInfo(login.LoginProvider, login.ProviderKey, login.DisplayName ?? user.Email))
                    .ToList();
            return Task.FromResult(list);
        }

        public Task<User> FindByLoginAsync(string loginProvider, string providerKey,
            CancellationToken cancellationToken)
        {
            return _session.Query<UserLogin>()
                .Where(
                    userLogin =>
                        userLogin.ProviderKey == providerKey &&
                        userLogin.LoginProvider == loginProvider)
                .Select(x => x.User)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public Task SetPasswordHashAsync(User user, string passwordHash, CancellationToken cancellationToken)
        {
            user.PasswordHash = Encoding.UTF8.GetBytes(passwordHash);
            user.PasswordSalt = null;
            user.CurrentEncryption = "IdentityV3";
            return Task.CompletedTask;
        }

        public async Task<string> GetPasswordHashAsync(User user, CancellationToken cancellationToken)
        {
            if (await HasPasswordAsync(user, cancellationToken))
                return Encoding.UTF8.GetString(user.PasswordHash);
            return null;
        }

        public Task<bool> HasPasswordAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user?.PasswordHash?.Any() == true);
        }

        public Task SetTwoFactorEnabledAsync(User user, bool enabled, CancellationToken cancellationToken)
        {
            user.TwoFactorAuthEnabled = enabled;
            return Task.CompletedTask;
        }

        public Task<bool> GetTwoFactorEnabledAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.TwoFactorAuthEnabled);
        }

        public Task ReplaceCodesAsync(User user, IEnumerable<string> recoveryCodes,
            CancellationToken cancellationToken)
        {
            user.TwoFactorRecoveryCodes = string.Join(";", recoveryCodes);
            return Task.CompletedTask;
        }

        public async Task<bool> RedeemCodeAsync(User user, string code, CancellationToken cancellationToken)
        {
            var codes = GetUserCodes(user);
            var matchingCode = codes.FirstOrDefault(existingCode =>
                string.Equals(code, existingCode, StringComparison.OrdinalIgnoreCase));
            if (matchingCode == null)
                return false;
            codes.Remove(matchingCode);
            await ReplaceCodesAsync(user, codes, cancellationToken);
            return true;
        }

        private static List<string> GetUserCodes(User user)
        {
            var codes = (user.TwoFactorRecoveryCodes ?? string.Empty).Split(";").ToList();
            return codes;
        }

        public Task<int> CountCodesAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(GetUserCodes(user).Count);
        }

        public Task SetPhoneNumberAsync(User user, string phoneNumber, CancellationToken cancellationToken)
        {
            user.PhoneNumber = phoneNumber;
            return Task.CompletedTask;
        }

        public Task<string> GetPhoneNumberAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PhoneNumber);
        }

        public Task<bool> GetPhoneNumberConfirmedAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PhoneNumberConfirmed);
        }

        public Task SetPhoneNumberConfirmedAsync(User user, bool confirmed, CancellationToken cancellationToken)
        {
            user.PhoneNumberConfirmed = confirmed;
            return Task.CompletedTask;
        }

        public Task SetAuthenticatorKeyAsync(User user, string key, CancellationToken cancellationToken)
        {
            user.AuthenticatorKey = key;
            return Task.CompletedTask;
        }

        public Task<string> GetAuthenticatorKeyAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.AuthenticatorKey);
        }

        public Task<DateTimeOffset?> GetLockoutEndDateAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.LockoutEndDate);
        }

        public Task SetLockoutEndDateAsync(User user, DateTimeOffset? lockoutEnd,
            CancellationToken cancellationToken)
        {
            user.LockoutEndDate = lockoutEnd;
            return Task.CompletedTask;
        }

        public Task<int> IncrementAccessFailedCountAsync(User user, CancellationToken cancellationToken)
        {
            var newAmount = user.AccessFailedCount + 1;
            user.AccessFailedCount = newAmount;
            return Task.FromResult(newAmount);
        }

        public Task ResetAccessFailedCountAsync(User user, CancellationToken cancellationToken)
        {
            user.AccessFailedCount = 0;
            return Task.CompletedTask;
        }

        public Task<int> GetAccessFailedCountAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.AccessFailedCount);
        }

        public Task<bool> GetLockoutEnabledAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.LockoutEnabled);
        }

        public Task SetLockoutEnabledAsync(User user, bool enabled, CancellationToken cancellationToken)
        {
            user.LockoutEnabled = enabled;
            return Task.CompletedTask;
        }

        public async Task SetTokenAsync(User user, string loginProvider, string name, string value,
            CancellationToken cancellationToken)
        {
            await _session.TransactAsync(async (session, token) =>
            {
                var existingToken = GetExistingToken(user, loginProvider, name);
                if (existingToken != null)
                {
                    user.UserTokens.Remove(existingToken);
                    await session.DeleteAsync(existingToken, token);
                }

                var userToken = new UserToken
                {
                    User = user,
                    Name = name,
                    Value = value,
                    LoginProvider = loginProvider
                };
                user.UserTokens.Add(userToken);
                await session.SaveAsync(userToken, token);
            }, cancellationToken);
        }

        private static UserToken GetExistingToken(User user, string loginProvider, string name)
        {
            return user.UserTokens.FirstOrDefault(x => x.LoginProvider == loginProvider && x.Name == name);
        }

        public async Task RemoveTokenAsync(User user, string loginProvider, string name,
            CancellationToken cancellationToken)
        {
            await _session.TransactAsync(async (session, token) =>
            {
                var existingToken = GetExistingToken(user, loginProvider, name);
                if (existingToken == null)
                    return;
                user.UserTokens.Remove(existingToken);
                await session.DeleteAsync(existingToken, token);
            }, cancellationToken);
        }

        public Task<string> GetTokenAsync(User user, string loginProvider, string name,
            CancellationToken cancellationToken)
        {
            var existingToken = GetExistingToken(user, loginProvider, name);
            return Task.FromResult(existingToken?.Value);
        }

        public Task SetSecurityStampAsync(User user, string stamp, CancellationToken cancellationToken)
        {
            user.SecurityStamp = stamp;
            return Task.CompletedTask;
        }

        public Task<string> GetSecurityStampAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.SecurityStamp ?? string.Empty);
        }
    }
}