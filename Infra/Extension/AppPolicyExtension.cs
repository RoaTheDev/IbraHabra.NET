using System.Collections.Immutable;
using IbraHabra.NET.Domain.Entity;
using IbraHabra.NET.Infra.Persistent;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using OpenIddict.Server;

namespace IbraHabra.NET.Infra.Extension;

public static class AppPolicyExtension
{
    public static void AddIdentityConfig(this IServiceCollection services, IConfiguration config)
    {
        services.AddIdentity<User, Role>(options =>
            {
                options.Password.RequireDigit = config.GetValue("Identity:Password:RequireDigit", true);
                options.Password.RequireLowercase = config.GetValue("Identity:Password:RequireLowercase", true);
                options.Password.RequireNonAlphanumeric =
                    config.GetValue("Identity:Password:RequireNonAlphanumeric", true);
                options.Password.RequireUppercase = config.GetValue("Identity:Password:RequireUppercase", true);
                options.Password.RequiredLength = config.GetValue("Identity:Password:RequiredLength", 8);
                options.Password.RequiredUniqueChars = config.GetValue("Identity:Password:RequiredUniqueChars", 1);

                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                options.SignIn.RequireConfirmedEmail =
                    config.GetValue("Identity:Login:Require_Email_Confirmation", true);
                options.SignIn.RequireConfirmedPhoneNumber =
                    config.GetValue("Identity:Login:Require_PhoneNumber_Confirmation", false);
                options.SignIn.RequireConfirmedAccount =
                    config.GetValue("Identity:Login:Require_Account_Confirmation", false);
                Console.WriteLine(options.SignIn);
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddApiEndpoints()
            .AddDefaultTokenProviders();
    }

    public static void AddOpenIdDictConfig(this IServiceCollection services)
    {
        services.AddOpenIddict()
            .AddCore(opts => opts.UseEntityFrameworkCore().UseDbContext<AppDbContext>())
            .AddServer(opts =>
            {
                opts.SetAuthorizationEndpointUris("/connect/authorize")
                    .SetTokenEndpointUris("/connect/token")
                    .SetEndSessionEndpointUris("/connect/logout")
                    .SetUserInfoEndpointUris("/connect/userinfo")
                    .SetRevocationEndpointUris("/connect/revocation");

                opts.AllowAuthorizationCodeFlow()
                    .AllowRefreshTokenFlow();

                opts.RequireProofKeyForCodeExchange();

                opts.UseReferenceAccessTokens();
                opts.UseReferenceRefreshTokens();

                opts.RegisterScopes(
                    OpenIddictConstants.Scopes.Email,
                    OpenIddictConstants.Scopes.Profile,
                    OpenIddictConstants.Scopes.Roles
                );

                opts.SetAccessTokenLifetime(TimeSpan.FromMinutes(10));
                opts.SetRefreshTokenLifetime(TimeSpan.FromDays(7));
                opts.AddDevelopmentEncryptionCertificate()
                    .AddDevelopmentSigningCertificate();

                opts.UseAspNetCore()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableTokenEndpointPassthrough()
                    .EnableEndSessionEndpointPassthrough()
                    .EnableUserInfoEndpointPassthrough()
                    .EnableStatusCodePagesIntegration();

                // Custom claims
                opts.AddEventHandler<OpenIddictServerEvents.ProcessSignInContext>(builder =>
                    builder.UseInlineHandler(async context =>
                    {
                        var userManager = context.Transaction.GetHttpRequest()!.HttpContext
                            .RequestServices.GetRequiredService<UserManager<User>>();

                        if (context.Principal != null)
                        {
                            var user = await userManager.FindByIdAsync(
                                context.Principal.GetClaim(OpenIddictConstants.Claims.Subject) ?? string.Empty);

                            if (user != null)
                            {
                                var roles = await userManager.GetRolesAsync(user);
                                context.Principal.SetClaims(OpenIddictConstants.Claims.Role,
                                    (ImmutableArray<string>)roles);
                            }
                        }
                    }));

                opts.AddEventHandler<OpenIddictServerEvents.HandleTokenRequestContext>(builders =>
                    builders.UseInlineHandler(async context =>
                    {
                        if (!context.Request.IsRefreshTokenGrantType()) return;

                        var manager = context.Transaction.GetHttpRequest()!
                            .HttpContext.RequestServices.GetRequiredService<IOpenIddictTokenManager>();

                        var token = await manager.FindByReferenceIdAsync(context.Request.RefreshToken ?? string.Empty,
                            CancellationToken.None);
                        if (token == null)
                        {
                            context.Reject(
                                error: OpenIddictConstants.Errors.InvalidGrant,
                                description: "The refresh token is no longer valid.");
                            return;
                        }

                        if (string.Equals(await manager.GetStatusAsync(token, CancellationToken.None),
                                OpenIddictConstants.Statuses.Redeemed, StringComparison.Ordinal))
                        {
                            // 🚨 SECURITY ALERT: Possible token theft
                            await manager.TryRevokeAsync(token, CancellationToken.None);

                            // Optionally revoke all tokens for this subject
                            var subject = await manager.GetSubjectAsync(token, CancellationToken.None);
                            if (subject != null)
                                await foreach (var t in manager.FindBySubjectAsync(subject, CancellationToken.None))
                                {
                                    await manager.TryRevokeAsync(t, CancellationToken.None);
                                }

                            context.Reject(
                                error: OpenIddictConstants.Errors.InvalidGrant,
                                description: "Possible token theft detected. All sessions revoked.");
                            return;
                        }

                        await manager.TryRedeemAsync(token, CancellationToken.None);
                    }));
                
                opts.AddEventHandler<OpenIddictServerEvents.ApplyTokenResponseContext>(builders =>
                    builders.UseInlineHandler(async context =>
                    {
                        if (context.Request != null && context.Request.IsRefreshTokenGrantType())
                        {
                            var manager = context.Transaction.GetHttpRequest()
                                ?.HttpContext.RequestServices
                                .GetRequiredService<IOpenIddictTokenManager>();

                            if (context.Request.RefreshToken != null)
                            {
                                if (manager != null)
                                {
                                    var oldToken = await manager.FindByReferenceIdAsync(
                                        context.Request.RefreshToken);

                                    if (oldToken != null)
                                    {
                                        await manager.TryRevokeAsync(oldToken, CancellationToken.None);
                                    }
                                }
                            }

                        }
                    }));
            });
    }
}