using System.Collections.Immutable;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using IbraHabra.NET.Domain.Contract;
using IbraHabra.NET.Domain.Contract.Services;
using IbraHabra.NET.Domain.Entities;
using IbraHabra.NET.Infra.Persistent;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using OpenIddict.Server.AspNetCore;

namespace IbraHabra.NET.Infra.Extension.Configs;

public static class AppPolicyExtension
{
    public static void AddIdentityConfig(this IServiceCollection services, IOptions<JwtOptions> jwt,
        IOptions<IdentitySettingOptions> identitySettings)
    {
        var settings = identitySettings.Value;

        services.AddIdentity<User, Role>(options =>
            {
                options.Password.RequireDigit = settings.Password.RequireDigit;
                options.Password.RequireLowercase = settings.Password.RequireLowercase;
                options.Password.RequireNonAlphanumeric = settings.Password.RequireNonAlphanumeric;
                options.Password.RequireUppercase = settings.Password.RequireUppercase;
                options.Password.RequiredLength = settings.Password.RequiredLength;
                options.Password.RequiredUniqueChars = settings.Password.RequiredUniqueChars;

                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(settings.Lockout.DefaultLockoutMinutes);
                options.Lockout.MaxFailedAccessAttempts = settings.Lockout.MaxFailedAccessAttempts;
                options.Lockout.AllowedForNewUsers = settings.Lockout.AllowedForNewUsers;

                options.SignIn.RequireConfirmedEmail = settings.SignIn.RequireConfirmedEmail;
                options.SignIn.RequireConfirmedPhoneNumber = settings.SignIn.RequireConfirmedPhoneNumber;
                options.SignIn.RequireConfirmedAccount = settings.SignIn.RequireConfirmedAccount;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        var jwtSecret = jwt.Value.Secret;
        if (!string.IsNullOrEmpty(jwtSecret))
        {
            var key = Encoding.UTF8.GetBytes(jwtSecret);

            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = jwt.Value.RequireHttps;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = true,
                        ValidIssuer = jwt.Value.Issuer,
                        ValidateAudience = true,
                        ValidAudience = jwt.Value.Audience,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };
                });
        }

        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy =>
            {
                policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                policy.RequireRole("Admin", "Super");
            });

            options.AddPolicy("Client", policy =>
            {
                policy.AuthenticationSchemes.Add(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                policy.RequireAuthenticatedUser();
            });
        });
    }

    public static void AddOpenIdDictConfig(this IServiceCollection services, IConfiguration config,
        IWebHostEnvironment env)
    {
        services.AddOpenIddict()
            .AddCore(opts => opts.UseEntityFrameworkCore().UseDbContext<AppDbContext>())
            .AddServer(opts =>
            {
                opts.SetAuthorizationEndpointUris("/connect/authorize")
                    .SetTokenEndpointUris("/connect/token")
                    .SetEndSessionEndpointUris("/connect/logout")
                    .SetUserInfoEndpointUris("/connect/userinfo")
                    .SetRevocationEndpointUris("/connect/revocation")
                    .SetIntrospectionEndpointUris("/connect/introspect");

                opts.AllowAuthorizationCodeFlow()
                    .AllowRefreshTokenFlow()
                    .AllowClientCredentialsFlow();

                // SECURITY: Enable PKCE globally or ensure all public clients enforce it
                // Uncomment if all public clients should require PKCE:
                // opts.RequireProofKeyForCodeExchange();

                opts.UseReferenceAccessTokens();
                opts.UseReferenceRefreshTokens();

                opts.RegisterScopes(
                    OpenIddictConstants.Scopes.Email,
                    OpenIddictConstants.Scopes.Profile,
                    OpenIddictConstants.Scopes.Roles
                );

                // IMPROVED: Shorter refresh token lifetime
                opts.SetAccessTokenLifetime(TimeSpan.FromMinutes(30));
                opts.SetRefreshTokenLifetime(TimeSpan.FromDays(7)); // Changed from 14 to 7 days

                // PRODUCTION: Use proper certificates with asymmetric keys (RSA)
                if (env.IsProduction())
                {
                    var certPath = config["OpenIddict:Certificate:Path"];
                    var certPassword = config["OpenIddict:Certificate:Password"]
                                       ?? Environment.GetEnvironmentVariable("CERT_PASSWORD");

                    if (!string.IsNullOrEmpty(certPath) && File.Exists(certPath))
                    {
                        var certificate = X509CertificateLoader.LoadPkcs12FromFile(certPath,
                            !string.IsNullOrEmpty(certPassword) ? certPassword : null,
                            X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet);

                        opts.AddEncryptionCertificate(certificate)
                            .AddSigningCertificate(certificate);
                    }
                    else
                    {
                        // SECURITY FIX: Throw error instead of using dynamic keys
                        throw new InvalidOperationException(
                            "Production environment requires a certificate. " +
                            "Set OpenIddict:Certificate:Path in configuration or provide a certificate file.");
                    }
                }
                else
                {
                    // DEVELOPMENT: Use development certificates (also asymmetric)
                    opts.AddDevelopmentEncryptionCertificate()
                        .AddDevelopmentSigningCertificate();
                }

                opts.UseAspNetCore()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableTokenEndpointPassthrough()
                    .EnableEndSessionEndpointPassthrough()
                    .EnableUserInfoEndpointPassthrough()
                    .EnableErrorPassthrough()
                    .DisableTransportSecurityRequirement();

                // SECURITY: Add token validation for security stamp
                opts.AddEventHandler<OpenIddictServerEvents.ValidateTokenContext>(builder =>
                    builder.UseInlineHandler(async context =>
                    {
                        if (context.Principal == null) return;

                        var userManager = context.Transaction.GetHttpRequest()
                            ?.HttpContext.RequestServices.GetRequiredService<UserManager<User>>();

                        if (userManager == null) return;

                        var userId = context.Principal.GetClaim(OpenIddictConstants.Claims.Subject);
                        if (string.IsNullOrEmpty(userId)) return;

                        var user = await userManager.FindByIdAsync(userId);
                        if (user == null)
                        {
                            context.Reject(
                                error: OpenIddictConstants.Errors.InvalidToken,
                                description: "The token is no longer valid.");
                            return;
                        }

                        // Validate security stamp
                        var tokenStamp = context.Principal.FindFirst("AspNet.Identity.SecurityStamp")?.Value;
                        var currentStamp = await userManager.GetSecurityStampAsync(user);

                        if (tokenStamp != currentStamp)
                        {
                            context.Reject(
                                error: OpenIddictConstants.Errors.InvalidToken,
                                description: "The token is no longer valid.");
                        }
                    }));

                //  THIS EVENT HANDLER FOR CLIENT AUTHENTICATION
                opts.AddEventHandler<OpenIddictServerEvents.ValidateTokenRequestContext>(builder =>
                    builder.UseInlineHandler(async context =>
                    {
                        // Only validate for confidential clients
                        if (context.Request.ClientId == null)
                            return;

                        var clientRepo = context.Transaction.GetHttpRequest()!
                            .HttpContext.RequestServices.GetRequiredService<IRepo<OauthApplication, string>>();

                        var secretHasher = context.Transaction.GetHttpRequest()!
                            .HttpContext.RequestServices.GetRequiredService<IClientSecretHasher>();

                        var client = await clientRepo.GetViaConditionAsync(c =>
                            c.ClientId == context.Request.ClientId && c.IsActive);

                        if (client == null)
                        {
                            context.Reject(
                                error: OpenIddictConstants.Errors.InvalidClient,
                                description: "The client application was not found.");
                            return;
                        }

                        // Public clients don't need authentication
                        if (client.ClientType == OpenIddictConstants.ClientTypes.Public)
                            return;

                        // Confidential clients must authenticate
                        var clientSecret = context.Request.ClientSecret;
                        if (string.IsNullOrEmpty(clientSecret) || string.IsNullOrEmpty(client.ClientSecret))
                        {
                            context.Reject(
                                error: OpenIddictConstants.Errors.InvalidClient,
                                description: "Client authentication failed.");
                            return;
                        }

                        // ✅ VERIFY THE HASHED SECRET
                        if (!secretHasher.VerifySecret(clientSecret, client.ClientSecret))
                        {
                            context.Reject(
                                error: OpenIddictConstants.Errors.InvalidClient,
                                description: "Client authentication failed.");
                            return;
                        }
                    }));
                // Custom claims handler
                opts.AddEventHandler<OpenIddictServerEvents.ProcessSignInContext>(builder =>
                    builder.UseInlineHandler(async context =>
                    {
                        var userManager = context.Transaction.GetHttpRequest()!.HttpContext
                            .RequestServices.GetRequiredService<UserManager<User>>();

                        if (context.Principal != null)
                        {
                            var userId = context.Principal.GetClaim(OpenIddictConstants.Claims.Subject);
                            if (string.IsNullOrEmpty(userId)) return;

                            var user = await userManager.FindByIdAsync(userId);
                            if (user != null)
                            {
                                var roles = await userManager.GetRolesAsync(user);
                                context.Principal.SetClaims(OpenIddictConstants.Claims.Role,
                                    roles.ToImmutableArray());

                                // Add security stamp to token for validation
                                var securityStamp = await userManager.GetSecurityStampAsync(user);
                                context.Principal.SetClaim("AspNet.Identity.SecurityStamp", securityStamp);
                            }
                        }
                    }));

                // Refresh token rotation with reuse detection
                opts.AddEventHandler<OpenIddictServerEvents.HandleTokenRequestContext>(builders =>
                    builders.UseInlineHandler(async context =>
                    {
                        if (!context.Request.IsRefreshTokenGrantType()) return;

                        var manager = context.Transaction.GetHttpRequest()!
                            .HttpContext.RequestServices.GetRequiredService<IOpenIddictTokenManager>();

                        var token = await manager.FindByReferenceIdAsync(
                            context.Request.RefreshToken ?? string.Empty, CancellationToken.None);

                        if (token == null)
                        {
                            context.Reject(
                                error: OpenIddictConstants.Errors.InvalidGrant,
                                description: "The refresh token is invalid.");
                            return;
                        }

                        // Token reuse detection
                        if (string.Equals(await manager.GetStatusAsync(token, CancellationToken.None),
                                OpenIddictConstants.Statuses.Redeemed, StringComparison.Ordinal))
                        {
                            await manager.TryRevokeAsync(token, CancellationToken.None);

                            var subject = await manager.GetSubjectAsync(token, CancellationToken.None);
                            if (subject != null)
                            {
                                // Revoke all tokens for this user due to suspected token theft
                                await foreach (var t in manager.FindBySubjectAsync(subject, CancellationToken.None))
                                {
                                    await manager.TryRevokeAsync(t, CancellationToken.None);
                                }
                            }

                            context.Reject(
                                error: OpenIddictConstants.Errors.InvalidGrant,
                                description: "The refresh token is invalid.");
                            return;
                        }

                        await manager.TryRedeemAsync(token, CancellationToken.None);
                    }));

                // Revoke old refresh token after successful refresh
                opts.AddEventHandler<OpenIddictServerEvents.ApplyTokenResponseContext>(builders =>
                    builders.UseInlineHandler(async context =>
                    {
                        if (context.Request?.IsRefreshTokenGrantType() == true &&
                            context.Request.RefreshToken != null)
                        {
                            var manager = context.Transaction.GetHttpRequest()
                                ?.HttpContext.RequestServices
                                .GetRequiredService<IOpenIddictTokenManager>();

                            if (manager != null)
                            {
                                var oldToken = await manager.FindByReferenceIdAsync(
                                    context.Request.RefreshToken, CancellationToken.None);

                                if (oldToken != null)
                                {
                                    await manager.TryRevokeAsync(oldToken, CancellationToken.None);
                                }
                            }
                        }
                    }));
            });

        // OPTIONAL: Add rate limiting for token endpoint
        // services.AddRateLimiter(options =>
        // {
        //     options.AddFixedWindowLimiter("token", opt =>
        //     {
        //         opt.Window = TimeSpan.FromMinutes(1);
        //         opt.PermitLimit = 10;
        //     });
        // });
    }
}