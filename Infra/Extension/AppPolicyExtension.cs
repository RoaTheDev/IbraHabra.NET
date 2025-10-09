using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using IbraHabra.NET.Domain.Entities;
using IbraHabra.NET.Infra.Persistent;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using OpenIddict.Server.AspNetCore;

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
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        // ADMIN JWT - Keep Symmetric (HMAC-SHA256)
        var jwtSecret = config["JWT:SECRET"] ?? Environment.GetEnvironmentVariable("JWT_SECRET");
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
                    options.RequireHttpsMetadata = config.GetValue("JWT:REQUIRE_HTTPS", true);
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = true,
                        ValidIssuer = config["JWT:ISSUER"] ?? "IbraHabra",
                        ValidateAudience = true,
                        ValidAudience = config["JWT:AUDIENCE"] ?? "IbraHabra.Domain.Coordinator",
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

                opts.RequireProofKeyForCodeExchange();

                opts.UseReferenceAccessTokens();
                opts.UseReferenceRefreshTokens();

                opts.RegisterScopes(
                    OpenIddictConstants.Scopes.Email,
                    OpenIddictConstants.Scopes.Profile,
                    OpenIddictConstants.Scopes.Roles
                );

                opts.SetAccessTokenLifetime(TimeSpan.FromMinutes(30));
                opts.SetRefreshTokenLifetime(TimeSpan.FromDays(14));

                // PRODUCTION: Use proper certificates with asymmetric keys (RSA)
                if (env.IsProduction())
                {
                    var certPath = config["OpenIddict:Certificate:Path"];
                    var certPassword = config["OpenIddict:Certificate:Password"]
                                       ?? Environment.GetEnvironmentVariable("CERT_PASSWORD");

                    if (!string.IsNullOrEmpty(certPath) && File.Exists(certPath))
                    {
                        // Use X509CertificateLoader (new API in .NET 8+)

                        var certificate = X509CertificateLoader.LoadPkcs12FromFile(certPath,
                            !string.IsNullOrEmpty(certPassword) ? certPassword : null,
                            X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet);

                        opts.AddEncryptionCertificate(certificate)
                            .AddSigningCertificate(certificate);
                    }
                    else
                    {
                        // Generate RSA keys if no certificate provided
                        var rsa = RSA.Create(2048);
                        var signingKey = new RsaSecurityKey(rsa) { KeyId = Guid.NewGuid().ToString() };
                        opts.AddSigningKey(signingKey);

                        var encryptionRsa = RSA.Create(2048);
                        var encryptionKey = new RsaSecurityKey(encryptionRsa) { KeyId = Guid.NewGuid().ToString() };
                        opts.AddEncryptionKey(encryptionKey);

                        Console.WriteLine("⚠️  WARNING: Using dynamically generated RSA keys. " +
                                          "Keys will change on restart. Use persistent certificates in production!");
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
                    .EnableStatusCodePagesIntegration();

                // Custom claims handler
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
                                    roles.ToImmutableArray());
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
                                description: "The refresh token is no longer valid.");
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
                                await foreach (var t in manager.FindBySubjectAsync(subject, CancellationToken.None))
                                {
                                    await manager.TryRevokeAsync(t, CancellationToken.None);
                                }
                            }

                            context.Reject(
                                error: OpenIddictConstants.Errors.InvalidGrant,
                                description: "Possible token theft detected. All sessions revoked.");
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
    }
}