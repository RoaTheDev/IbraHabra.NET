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
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using OpenIddict.Server.AspNetCore;

namespace IbraHabra.NET.Infra.Extension.Configs;

public static class AppPolicyExtension
{
    public static void AddIdentityConfig(this IServiceCollection services, IConfiguration config)
    {
        var jwt = config.GetSection("JWT").Get<JwtOptions>() ?? null;
        var settings = config.GetSection("IDENTITY").Get<IdentitySettingOptions>() ?? null;


        services.AddIdentity<User, Role>(options =>
            {
                if (settings != null)
                {
                    options.Password.RequireDigit = settings.Password.RequireDigit;
                    options.Password.RequireLowercase = settings.Password.RequireLowercase;
                    options.Password.RequireNonAlphanumeric = settings.Password.RequireNonAlphanumeric;
                    options.Password.RequireUppercase = settings.Password.RequireUppercase;
                    options.Password.RequiredLength = settings.Password.RequiredLength;
                    options.Password.RequiredUniqueChars = settings.Password.RequiredUniqueChars;

                    options.Lockout.DefaultLockoutTimeSpan =
                        TimeSpan.FromMinutes(settings.Lockout.DefaultLockoutMinutes);
                    options.Lockout.MaxFailedAccessAttempts = settings.Lockout.MaxFailedAccessAttempts;
                    options.Lockout.AllowedForNewUsers = settings.Lockout.AllowedForNewUsers;

                    options.SignIn.RequireConfirmedEmail = settings.SignIn.RequireConfirmedEmail;
                    options.SignIn.RequireConfirmedPhoneNumber = settings.SignIn.RequireConfirmedPhoneNumber;
                    options.SignIn.RequireConfirmedAccount = settings.SignIn.RequireConfirmedAccount;
                }
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        if (jwt != null)
        {
            var jwtSecret = jwt.Secret;
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
                        options.RequireHttpsMetadata = jwt.RequireHttps;
                        options.SaveToken = true;
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = new SymmetricSecurityKey(key),
                            ValidateIssuer = true,
                            ValidIssuer = jwt.Issuer,
                            ValidateAudience = true,
                            ValidAudience = jwt.Audience,
                            ValidateLifetime = true,
                            ClockSkew = TimeSpan.Zero
                        };
                    });
            }
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
                // Endpoint configuration
                opts.SetAuthorizationEndpointUris("/connect/authorize")
                    .SetTokenEndpointUris("/connect/token")
                    .SetEndSessionEndpointUris("/connect/logout")
                    .SetUserInfoEndpointUris("/connect/userinfo")
                    .SetRevocationEndpointUris("/connect/revocation")
                    .SetIntrospectionEndpointUris("/connect/introspect");

                // Grant types
                opts.AllowAuthorizationCodeFlow()
                    .AllowRefreshTokenFlow()
                    .AllowClientCredentialsFlow();


                // Use reference tokens (more secure, can be revoked)
                opts.UseReferenceAccessTokens();
                opts.UseReferenceRefreshTokens();

                // Register scopes
                opts.RegisterScopes(
                    OpenIddictConstants.Scopes.OpenId,
                    OpenIddictConstants.Scopes.Email,
                    OpenIddictConstants.Scopes.Profile,
                    OpenIddictConstants.Scopes.Roles,
                    OpenIddictConstants.Scopes.OfflineAccess
                );

                // Token lifetimes
                if (env.IsProduction())
                {
                    // PRODUCTION: Shorter lifetimes
                    opts.SetAccessTokenLifetime(TimeSpan.FromMinutes(15));
                    opts.SetRefreshTokenLifetime(TimeSpan.FromDays(7));
                    opts.SetAuthorizationCodeLifetime(TimeSpan.FromMinutes(5));
                }
                else
                {
                    // DEVELOPMENT: Longer lifetimes for easier testing
                    opts.SetAccessTokenLifetime(TimeSpan.FromHours(1));
                    opts.SetRefreshTokenLifetime(TimeSpan.FromDays(14));
                    opts.SetAuthorizationCodeLifetime(TimeSpan.FromMinutes(10));
                }

                // Certificate configuration
                if (env.IsProduction())
                {
                    ConfigureProductionCertificates(opts, config);
                }
                else
                {
                    // DEVELOPMENT: Use development certificates
                    opts.AddDevelopmentEncryptionCertificate()
                        .AddDevelopmentSigningCertificate();
                }

                // ASP.NET Core integration
                opts.UseAspNetCore()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableTokenEndpointPassthrough()
                    .EnableEndSessionEndpointPassthrough()
                    .EnableUserInfoEndpointPassthrough()
                    .EnableErrorPassthrough()
                    .EnableEndUserVerificationEndpointPassthrough();


                // ====================================
                // EVENT HANDLERS
                // ====================================

                AddSecurityStampValidation(opts);

                AddClientAuthentication(opts);

                AddCustomClaimsHandler(opts);

                AddRefreshTokenRotation(opts);

                AddRefreshTokenCleanup(opts);

                AddTokenIssuanceLogging(opts);

                AddFailedAuthTracking(opts);

                AddConditionalPkceValidation(opts);
            })
            .AddValidation(opts =>
            {
                // Import the configuration from the local OpenIddict server instance
                opts.UseLocalServer();

                // Register the ASP.NET Core host
                opts.UseAspNetCore();
            });

        // PRODUCTION: Add rate limiting
        if (env.IsProduction())
        {
            AddRateLimiting(services);
        }
    }

    private static void ConfigureProductionCertificates(OpenIddictServerBuilder opts, IConfiguration config)
    {
        var certPath = config["OpenIddict:Certificate:Path"];
        var certPassword = config["OpenIddict:Certificate:Password"]
                           ?? Environment.GetEnvironmentVariable("CERT_PASSWORD");

        if (!string.IsNullOrEmpty(certPath) && File.Exists(certPath))
        {
            var certificate = X509CertificateLoader.LoadPkcs12FromFile(
                certPath,
                !string.IsNullOrEmpty(certPassword) ? certPassword : null,
                X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet);

            opts.AddEncryptionCertificate(certificate)
                .AddSigningCertificate(certificate);
        }
        else
        {
            throw new InvalidOperationException(
                "Production environment requires a certificate. " +
                "Set OpenIddict:Certificate:Path in configuration or provide a certificate file. " +
                "Generate a certificate using: " +
                "dotnet dev-certs https -ep certificate.pfx -p YourPassword --trust");
        }
    }

    private static void AddSecurityStampValidation(OpenIddictServerBuilder opts)
    {
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
    }

    private static void AddClientAuthentication(OpenIddictServerBuilder opts)
    {
        opts.AddEventHandler<OpenIddictServerEvents.ValidateTokenRequestContext>(builder =>
            builder.UseInlineHandler(async context =>
            {
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

                if (!secretHasher.VerifySecret(clientSecret, client.ClientSecret))
                {
                    context.Reject(
                        error: OpenIddictConstants.Errors.InvalidClient,
                        description: "Client authentication failed.");
                }
            }));
    }

    private static void AddCustomClaimsHandler(OpenIddictServerBuilder opts)
    {
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
    }

    private static void AddRefreshTokenRotation(OpenIddictServerBuilder opts)
    {
        opts.AddEventHandler<OpenIddictServerEvents.HandleTokenRequestContext>(builder =>
            builder.UseInlineHandler(async context =>
            {
                if (!context.Request.IsRefreshTokenGrantType()) return;

                var manager = context.Transaction.GetHttpRequest()!
                    .HttpContext.RequestServices.GetRequiredService<IOpenIddictTokenManager>();

                var logger = context.Transaction.GetHttpRequest()!
                    .HttpContext.RequestServices
                    .GetRequiredService<ILogger<OpenIddictServerEvents.HandleTokenRequestContext>>();

                var token = await manager.FindByReferenceIdAsync(
                    context.Request.RefreshToken ?? string.Empty, CancellationToken.None);

                if (token == null)
                {
                    logger.LogWarning("Refresh token not found: {Token}", context.Request.RefreshToken);
                    context.Reject(
                        error: OpenIddictConstants.Errors.InvalidGrant,
                        description: "The refresh token is invalid.");
                    return;
                }

                // Token reuse detection
                if (string.Equals(await manager.GetStatusAsync(token, CancellationToken.None),
                        OpenIddictConstants.Statuses.Redeemed, StringComparison.Ordinal))
                {
                    var subject = await manager.GetSubjectAsync(token, CancellationToken.None);
                    logger.LogWarning("Refresh token reuse detected for user: {Subject}", subject);

                    await manager.TryRevokeAsync(token, CancellationToken.None);

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
    }

    private static void AddRefreshTokenCleanup(OpenIddictServerBuilder opts)
    {
        opts.AddEventHandler<OpenIddictServerEvents.ApplyTokenResponseContext>(builder =>
            builder.UseInlineHandler(async context =>
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
    }

    private static void AddTokenIssuanceLogging(OpenIddictServerBuilder opts)
    {
        opts.AddEventHandler<OpenIddictServerEvents.ProcessSignInContext>(builder =>
            builder.UseInlineHandler(context =>
            {
                var logger = context.Transaction.GetHttpRequest()!
                    .HttpContext.RequestServices
                    .GetRequiredService<ILogger<OpenIddictServerEvents.ProcessSignInContext>>();

                var subject = context.Principal?.GetClaim(OpenIddictConstants.Claims.Subject);
                var clientId = context.Request.ClientId;
                var grantType = context.Request.GrantType;

                logger.LogInformation(
                    "Token issued - Subject: {Subject}, Client: {ClientId}, GrantType: {GrantType}",
                    subject, clientId, grantType);

                return default;
            }));
    }

    private static void AddFailedAuthTracking(OpenIddictServerBuilder opts)
    {
        opts.AddEventHandler<OpenIddictServerEvents.ProcessErrorContext>(builder =>
            builder.UseInlineHandler(context =>
            {
                var logger = context.Transaction.GetHttpRequest()!
                    .HttpContext.RequestServices
                    .GetRequiredService<ILogger<OpenIddictServerEvents.ProcessErrorContext>>();

                if (context.Error == OpenIddictConstants.Errors.InvalidGrant ||
                    context.Error == OpenIddictConstants.Errors.InvalidClient)
                {
                    var clientId = context.Request?.ClientId;
                    logger.LogWarning(
                        "Authentication failed - Client: {ClientId}, Error: {Error}, Description: {Description}",
                        clientId, context.Error, context.ErrorDescription);

                    // TODO: Implement IP-based rate limiting or account lockout here
                }

                return default;
            }));
    }

    private static void AddRateLimiting(IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            // Token endpoint rate limiting
            options.AddFixedWindowLimiter("token", opt =>
            {
                opt.Window = TimeSpan.FromMinutes(1);
                opt.PermitLimit = 10;
                opt.QueueLimit = 0;
            });

            // Authorization endpoint rate limiting
            options.AddFixedWindowLimiter("authorize", opt =>
            {
                opt.Window = TimeSpan.FromMinutes(1);
                opt.PermitLimit = 20;
                opt.QueueLimit = 0;
            });

            // Global rate limiting
            options.AddSlidingWindowLimiter("global", opt =>
            {
                opt.Window = TimeSpan.FromMinutes(1);
                opt.PermitLimit = 100;
                opt.QueueLimit = 0;
                opt.SegmentsPerWindow = 4;
            });
        });
    }

    private static void AddConditionalPkceValidation(OpenIddictServerBuilder opts)
    {
        opts.AddEventHandler<OpenIddictServerEvents.ValidateAuthorizationRequestContext>(builder =>
            builder.UseInlineHandler(async context =>
            {
                if (!context.Request.IsAuthorizationCodeGrantType())
                    return;

                var applicationManager = context.Transaction.GetHttpRequest()!
                    .HttpContext.RequestServices.GetRequiredService<IOpenIddictApplicationManager>();
                if (context.Request.ClientId != null)
                {
                    var client = await applicationManager.FindByClientIdAsync(context.Request.ClientId);
                    if (client == null)
                    {
                        return;
                    }

                    var clientType = await applicationManager.GetClientTypeAsync(client);
                    if (clientType == OpenIddictConstants.ClientTypes.Public)
                    {
                        if (string.IsNullOrEmpty(context.Request.CodeChallenge))
                        {
                            context.Reject(
                                error: OpenIddictConstants.Errors.InvalidRequest,
                                description: "PKCE is required for public clients.");
                            return;
                        }

                        var codeChallengeMethod = context.Request.CodeChallengeMethod ??
                                                  OpenIddictConstants.CodeChallengeMethods.Plain;
                        if (codeChallengeMethod != OpenIddictConstants.CodeChallengeMethods.Sha256)
                        {
                            context.Reject(
                                error: OpenIddictConstants.Errors.InvalidRequest,
                                description: "Only S256 code challenge method is supported.");
                        }
                    }
                }

                // For confidential clients, no PKCE check—proceed normally
            }));
    }
}