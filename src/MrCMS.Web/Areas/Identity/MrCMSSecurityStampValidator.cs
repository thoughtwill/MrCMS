using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MrCMS.Entities.People;
using MrCMS.Helpers;
using MrCMS.Services;

namespace MrCMS.Web.Areas.Identity;

public class MrCMSSecurityStampValidator : SecurityStampValidator<User>
{
    private readonly UserManager _userManager;

    public MrCMSSecurityStampValidator(IOptions<SecurityStampValidatorOptions> options,
        SignInManager<User> signInManager, ILoggerFactory logger, UserManager userManager)
        : base(options, signInManager, logger)
    {
        _userManager = userManager;
    }

    protected override async Task<User> VerifySecurityStamp(ClaimsPrincipal principal)
    {
        var user = (await _userManager.FindByEmailAsync(principal.Identity!.Name)).Unproxy();

        if (user == null)
            return null;

        var newPrincipal = await SignInManager.CreateUserPrincipalAsync(user);
        return principal.FindFirstValue(nameof(User.SecurityStamp)) !=
               newPrincipal.FindFirstValue(nameof(User.SecurityStamp)) ? null : user;
    }
}