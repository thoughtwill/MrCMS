using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MrCMS.Entities.People;
using MrCMS.Services;
using MrCMS.Services.Auth;

[assembly: HostingStartup(typeof(MrCMS.Web.Areas.Identity.IdentityHostingStartup))]
namespace MrCMS.Web.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
                services.ConfigureOptions<IdentityDefaultUIConfigureOptions<User>>();
                services.AddIdentity<User, UserRole>(options =>
                    {
                        // lockout
                        if (int.TryParse(context.Configuration["Auth:Lockout:MaxFailedAccessAttempts"],
                            out var maxFailedAccessAttempts))
                        {
                            options.Lockout.MaxFailedAccessAttempts = maxFailedAccessAttempts;
                        }

                        if (TimeSpan.TryParse(context.Configuration["Auth:Lockout:DefaultLockoutTimeSpan"],
                            out var defaultLockoutTimeSpan))
                        {
                            options.Lockout.DefaultLockoutTimeSpan = defaultLockoutTimeSpan;
                        }

                        if (bool.TryParse(context.Configuration["Auth:Lockout:AllowedForNewUsers"],
                            out var allowedForNewUsers))
                        {
                            options.Lockout.AllowedForNewUsers = allowedForNewUsers;
                        }

                        // password
                        if (int.TryParse(context.Configuration["Auth:Password:RequiredLength"], out var requiredLength))
                        {
                            options.Password.RequiredLength = requiredLength;
                        }

                        if (bool.TryParse(context.Configuration["Auth:Password:RequireDigit"], out var requireDigit))
                        {
                            options.Password.RequireDigit = requireDigit;
                        }

                        if (bool.TryParse(context.Configuration["Auth:Password:RequireLowercase"],
                            out var requireLowercase))
                        {
                            options.Password.RequireLowercase = requireLowercase;
                        }

                        if (bool.TryParse(context.Configuration["Auth:Password:RequireNonAlphanumeric"],
                            out var requireNonAlphanumeric))
                        {
                            options.Password.RequireNonAlphanumeric = requireNonAlphanumeric;
                        }

                        if (bool.TryParse(context.Configuration["Auth:Password:RequireUppercase"],
                            out var requireUppercase))
                        {
                            options.Password.RequireUppercase = requireUppercase;
                        }

                        // sign in
                        if (bool.TryParse(context.Configuration["Auth:SignIn:RequireConfirmedEmail"],
                            out var requireConfirmedEmail))
                        {
                            options.SignIn.RequireConfirmedEmail = requireConfirmedEmail;
                        }

                        if (bool.TryParse(context.Configuration["Auth:SignIn:RequireConfirmedPhoneNumber"],
                            out var requireConfirmedPhoneNumber))
                        {
                            options.SignIn.RequireConfirmedPhoneNumber = requireConfirmedPhoneNumber;
                        }

                        options.User.RequireUniqueEmail = true;
                        options.ClaimsIdentity.UserNameClaimType = nameof(User.Email);
                        options.ClaimsIdentity.SecurityStampClaimType = nameof(User.SecurityStamp);
                    })
                    .AddUserStore<UserStore>()
                    .AddRoleStore<RoleStore>()
                    .AddUserManager<UserManager>()
                    .AddSignInManager<SignInManager>()
                    .AddDefaultTokenProviders();

                services.AddScoped<IPasswordHasher<User>, MrCMSPasswordHasher>();
                // services.AddScoped<IEmailSender, MrCMSEmailSender>();
                
                services.Configure<SecurityStampValidatorOptions>(o =>
                {
                    o.ValidationInterval = TimeSpan.FromSeconds(60);
                });
                
                services.Configure<DataProtectionTokenProviderOptions>(o =>
                {
                    o.TokenLifespan = TimeSpan.FromHours(3);
                });
                
                services.AddScoped<ISecurityStampValidator, MrCMSSecurityStampValidator>();
            });
        }
    }
}