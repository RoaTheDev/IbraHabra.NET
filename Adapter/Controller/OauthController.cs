using System.Collections.Immutable;
using System.Security.Claims;
using IbraHabra.NET.Application.Utils;
using IbraHabra.NET.Domain.Contract;
using IbraHabra.NET.Domain.Entities;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace IbraHabra.NET.Adapter.Controller;

[ApiController]
public class OauthController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IOpenIddictAuthorizationManager _authorizationManager;
    private readonly IOpenIddictScopeManager _scopeManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IRepo<OauthApplication, string> _clientRepo;

    public OauthController(
        IOpenIddictApplicationManager applicationManager,
        IOpenIddictAuthorizationManager authorizationManager,
        IOpenIddictScopeManager scopeManager,
        SignInManager<User> signInManager,
        UserManager<User> userManager,
        IRepo<OauthApplication, string> clientRepo)
    {
        _applicationManager = applicationManager;
        _authorizationManager = authorizationManager;
        _scopeManager = scopeManager;
        _signInManager = signInManager;
        _userManager = userManager;
        _clientRepo = clientRepo;
    }

    [HttpGet("~/connect/authorize")]
    [HttpPost("~/connect/authorize")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Authorize()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
                      throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        var application = await _clientRepo.GetViaConditionAsync(c => c.ClientId == request.ClientId! && c.IsActive) ??
                          throw new InvalidOperationException(
                              "Details concerning the calling client application cannot be found.");

        var authPolicy = ReadAuthPolicy.GetAuthPolicy(application.Properties);

        if (authPolicy.RequirePkce && string.IsNullOrEmpty(request.CodeChallenge))
        {
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] =
                        OpenIddictConstants.Errors.InvalidRequest,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                        "PKCE is required for this client."
                }!));
        }

        var result = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);

        if (!result.Succeeded)
        {
            if (request.HasPromptValue(OpenIddictConstants.PromptValues.None))
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] =
                            OpenIddictConstants.Errors.LoginRequired,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "The user is not logged in."
                    }!));
            }

            return Challenge(
                authenticationSchemes: IdentityConstants.ApplicationScheme,
                properties: new AuthenticationProperties
                {
                    RedirectUri = Request.PathBase + Request.Path + QueryString.Create(
                        Request.HasFormContentType ? Request.Form.ToList() : Request.Query.ToList())
                });
        }

        var user = await _userManager.GetUserAsync(result.Principal) ??
                   throw new InvalidOperationException("The user details cannot be retrieved.");

        var userId = await _userManager.GetUserIdAsync(user);

        var authorizations = await _authorizationManager.FindAsync(
            subject: userId,
            client: await _applicationManager.GetIdAsync(application),
            status: OpenIddictConstants.Statuses.Valid,
            type: OpenIddictConstants.AuthorizationTypes.Permanent,
            scopes: ImmutableArray.CreateRange(request.GetScopes()),
            cancellationToken: HttpContext.RequestAborted).ToListAsync();

        // Check for external consent requirement
        if (!authorizations.Any() &&
            await _applicationManager.HasConsentTypeAsync(application, OpenIddictConstants.ConsentTypes.External))
        {
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] =
                        OpenIddictConstants.Errors.ConsentRequired,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                        "The logged-in user is not allowed to access this client application."
                }!));
        }

        var principal = await _signInManager.CreateUserPrincipalAsync(user);

        principal.SetClaim(OpenIddictConstants.Claims.Subject, userId);

        if (!string.IsNullOrEmpty(user.UserName))
        {
            principal.SetClaim(OpenIddictConstants.Claims.Name, user.UserName);
        }

        if (!string.IsNullOrEmpty(user.Email))
        {
            principal.SetClaim(OpenIddictConstants.Claims.Email, user.Email);
        }

        principal.SetScopes(request.GetScopes());
        principal.SetResources(await _scopeManager.ListResourcesAsync(principal.GetScopes()).ToListAsync());

        // Handle authorization with proper consent check
        var authorization = authorizations.LastOrDefault();
        if (authorization == null)
        {
            // Only auto-approve for trusted clients (implicit consent)
            var consentType = await _applicationManager.GetConsentTypeAsync(application);
            if (consentType != OpenIddictConstants.ConsentTypes.Implicit)
            {
                // For explicit/external consent, return consent_required
                // You should redirect to a consent page in a real implementation
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] =
                            OpenIddictConstants.Errors.ConsentRequired,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "User consent is required."
                    }!));
            }

            // Create authorization only for implicit consent clients
            authorization = await _authorizationManager.CreateAsync(
                principal: principal,
                subject: userId,
                client: (await _applicationManager.GetIdAsync(application))!,
                type: OpenIddictConstants.AuthorizationTypes.Permanent,
                scopes: principal.GetScopes(),
                cancellationToken: HttpContext.RequestAborted);
        }

        principal.SetAuthorizationId(await _authorizationManager.GetIdAsync(authorization, HttpContext.RequestAborted));

        // Set claim destinations
        foreach (var claim in principal.Claims)
        {
            claim.SetDestinations(GetDestinations(claim));
        }

        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [HttpPost("~/connect/token")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
                      throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        if (request.IsAuthorizationCodeGrantType())
        {
            // Retrieve the claims principal stored in the authorization code
            var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            if (result.Principal == null)
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] =
                            OpenIddictConstants.Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "The authorization code is invalid."
                    }!));
            }

            // Use OpenIddict subject claim
            var userId = result.Principal.GetClaim(OpenIddictConstants.Claims.Subject);
            if (string.IsNullOrEmpty(userId))
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] =
                            OpenIddictConstants.Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "The authorization code is invalid."
                    }!));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] =
                            OpenIddictConstants.Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "The authorization code is invalid."
                    }!));
            }

            // Validate security stamp
            if (!await ValidateSecurityStampAsync(user, result.Principal))
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] =
                            OpenIddictConstants.Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "The authorization code is invalid."
                    }!));
            }

            // Ensure the user is still allowed to sign in
            if (!await _signInManager.CanSignInAsync(user))
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] =
                            OpenIddictConstants.Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "The authorization code is invalid."
                    }!));
            }

            foreach (var claim in result.Principal.Claims)
            {
                claim.SetDestinations(GetDestinations(claim));
            }

            return SignIn(result.Principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        if (request.IsRefreshTokenGrantType())
        {
            // Retrieve the claims principal stored in the refresh token
            var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            if (result.Principal == null)
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] =
                            OpenIddictConstants.Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "The refresh token is invalid."
                    }!));
            }

            // Use OpenIddict subject claim
            var userId = result.Principal.GetClaim(OpenIddictConstants.Claims.Subject);
            if (string.IsNullOrEmpty(userId))
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] =
                            OpenIddictConstants.Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "The refresh token is invalid."
                    }!));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] =
                            OpenIddictConstants.Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "The refresh token is invalid."
                    }!));
            }

            // Validate security stamp
            if (!await ValidateSecurityStampAsync(user, result.Principal))
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] =
                            OpenIddictConstants.Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "The refresh token is invalid."
                    }!));
            }

            // Ensure the user is still allowed to sign in
            if (!await _signInManager.CanSignInAsync(user))
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] =
                            OpenIddictConstants.Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "The refresh token is invalid."
                    }!));
            }

            foreach (var claim in result.Principal.Claims)
            {
                claim.SetDestinations(GetDestinations(claim));
            }

            return SignIn(result.Principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        if (request.IsClientCredentialsGrantType())
        {
            // Find the application
            var application = await _applicationManager.FindByClientIdAsync(request.ClientId);
            if (application == null)
            {
                throw new InvalidOperationException("The application cannot be found.");
            }

            // Validate requested scopes against allowed scopes
            var requestedScopes = request.GetScopes().ToList();
            var allowedScopes = await _applicationManager.GetPermissionsAsync(application);
            var allowedScopeNames = allowedScopes
                .Where(p => p.StartsWith(OpenIddictConstants.Permissions.Prefixes.Scope))
                .Select(p => p.Substring(OpenIddictConstants.Permissions.Prefixes.Scope.Length))
                .ToList();

            var unauthorizedScopes = requestedScopes.Except(allowedScopeNames).ToList();
            if (unauthorizedScopes.Any())
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] =
                            OpenIddictConstants.Errors.InvalidScope,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "The requested scopes are not allowed for this client."
                    }!));
            }

            // Create a new ClaimsPrincipal for the client application
            var identity = new ClaimsIdentity(
                OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                OpenIddictConstants.Claims.Name,
                OpenIddictConstants.Claims.Role);

            // Subject is mandatory
            identity.AddClaim(OpenIddictConstants.Claims.Subject,
                request.ClientId ?? throw new InvalidOperationException("Client ID is missing."));

            // Optionally add the client name
            var displayName = await _applicationManager.GetDisplayNameAsync(application);
            if (!string.IsNullOrEmpty(displayName))
            {
                identity.AddClaim(OpenIddictConstants.Claims.Name, displayName);
            }

            var principal = new ClaimsPrincipal(identity);
            principal.SetScopes(requestedScopes);
            principal.SetResources(await _scopeManager.ListResourcesAsync(principal.GetScopes()).ToListAsync());

            foreach (var claim in principal.Claims)
            {
                claim.SetDestinations(OpenIddictConstants.Destinations.AccessToken);
            }

            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        throw new InvalidOperationException("The specified grant type is not supported.");
    }


    [HttpGet("~/connect/logout")]
    [HttpPost("~/connect/logout")]
    public async Task<IActionResult> Logout()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
                      throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        // Try to get the current user from the id_token_hint or current session
        var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

        string? userId = null;
        if (result.Succeeded && result.Principal != null)
        {
            userId = result.Principal.FindFirst(OpenIddictConstants.Claims.Subject)?.Value;
        }

        // If no id_token_hint, try to get user from current session
        if (string.IsNullOrEmpty(userId))
        {
            var sessionResult = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);
            if (sessionResult.Succeeded && sessionResult.Principal != null)
            {
                var user = await _userManager.GetUserAsync(sessionResult.Principal);
                if (user != null)
                {
                    userId = await _userManager.GetUserIdAsync(user);
                }
            }
        }

        // Revoke all tokens for the user
        if (!string.IsNullOrEmpty(userId))
        {
            var tokenManager = HttpContext.RequestServices.GetRequiredService<IOpenIddictTokenManager>();
            await foreach (var token in tokenManager.FindBySubjectAsync(userId))
            {
                await tokenManager.TryRevokeAsync(token);
            }
        }

        // Sign out from Identity
        await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

        return SignOut(
            authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            properties: new AuthenticationProperties
            {
                RedirectUri = request.PostLogoutRedirectUri
            });
    }

    [HttpPost("~/connect/revocation")]
    [IgnoreAntiforgeryToken]
    public Task<IActionResult> Revoke()
    {
        // Revocation is handled automatically by OpenIddict
        // This endpoint just needs to exist for the framework to process the request
        return Task.FromResult<IActionResult>(Ok());
    }

    [HttpPost("~/connect/introspect")]
    [IgnoreAntiforgeryToken]
    public Task<IActionResult> Introspect()
    {
        // Introspection is handled automatically by OpenIddict
        // This endpoint just needs to exist for the framework to process the request
        return Task.FromResult<IActionResult>(Ok());
    }

    [HttpGet("~/connect/userinfo")]
    [HttpPost("~/connect/userinfo")]
    [Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
    public async Task<IActionResult> Userinfo()
    {
        var userId = User.GetClaim(OpenIddictConstants.Claims.Subject);
        if (string.IsNullOrEmpty(userId))
        {
            return Challenge(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] =
                        OpenIddictConstants.Errors.InvalidToken,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                        "The access token is missing required claims."
                }!));
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return Challenge(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] =
                        OpenIddictConstants.Errors.InvalidToken,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                        "The specified access token is no longer valid."
                }!));
        }

        var claims = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            [OpenIddictConstants.Claims.Subject] = userId
        };

        if (User.HasScope(OpenIddictConstants.Scopes.Profile))
        {
            claims[OpenIddictConstants.Claims.Name] = user.UserName ?? string.Empty;
        }

        if (User.HasScope(OpenIddictConstants.Scopes.Email))
        {
            claims[OpenIddictConstants.Claims.Email] = user.Email ?? string.Empty;
            claims[OpenIddictConstants.Claims.EmailVerified] = user.EmailConfirmed;
        }

        if (User.HasScope(OpenIddictConstants.Scopes.Roles))
        {
            claims[OpenIddictConstants.Claims.Role] = await _userManager.GetRolesAsync(user);
        }

        return Ok(claims);
    }


    private async Task<bool> ValidateSecurityStampAsync(User user, ClaimsPrincipal principal)
    {
        var securityStamp = principal.FindFirst("AspNet.Identity.SecurityStamp")?.Value;
        var currentStamp = await _userManager.GetSecurityStampAsync(user);
        return securityStamp == currentStamp;
    }

    private static IEnumerable<string> GetDestinations(Claim claim)
    {
        // Note: by default, claims are NOT automatically included in the access and identity tokens.
        // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
        // whether they should be included in access tokens, in identity tokens or in both.

        switch (claim.Type)
        {
            case OpenIddictConstants.Claims.Subject:
                yield return OpenIddictConstants.Destinations.AccessToken;
                yield return OpenIddictConstants.Destinations.IdentityToken;
                yield break;

            case OpenIddictConstants.Claims.Name:
                yield return OpenIddictConstants.Destinations.AccessToken;

                if (claim.Subject?.HasScope(OpenIddictConstants.Permissions.Scopes.Profile) == true)
                    yield return OpenIddictConstants.Destinations.IdentityToken;

                yield break;

            case OpenIddictConstants.Claims.Email:
                yield return OpenIddictConstants.Destinations.AccessToken;

                if (claim.Subject?.HasScope(OpenIddictConstants.Permissions.Scopes.Email) == true)
                    yield return OpenIddictConstants.Destinations.IdentityToken;

                yield break;

            case OpenIddictConstants.Claims.Role:
                yield return OpenIddictConstants.Destinations.AccessToken;

                if (claim.Subject?.HasScope(OpenIddictConstants.Permissions.Scopes.Roles) == true)
                    yield return OpenIddictConstants.Destinations.IdentityToken;

                yield break;

            // Never include the security stamp in the access and identity tokens, as it's a secret value
            case "AspNet.Identity.SecurityStamp":
                yield break;

            default:
                yield return OpenIddictConstants.Destinations.AccessToken;
                yield break;
        }
    }
}