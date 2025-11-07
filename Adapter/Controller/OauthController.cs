using System.Collections.Immutable;
using System.Security.Claims;
using IbraHabra.NET.Application.Dto.Request.User;
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
    private readonly ILogger<OauthController> _logger;

    public OauthController(
        IOpenIddictApplicationManager applicationManager,
        IOpenIddictAuthorizationManager authorizationManager,
        IOpenIddictScopeManager scopeManager,
        SignInManager<User> signInManager,
        UserManager<User> userManager,
        IRepo<OauthApplication, string> clientRepo,
        ILogger<OauthController> logger)
    {
        _applicationManager = applicationManager;
        _authorizationManager = authorizationManager;
        _scopeManager = scopeManager;
        _signInManager = signInManager;
        _userManager = userManager;
        _clientRepo = clientRepo;
        _logger = logger;
    }

    // Fix consent endpoint routes and add authorization
    [HttpGet("~/connect/consent")]
    [Authorize] // ← ADDED
    public async Task<IActionResult> GetConsent()
    {
        var request = HttpContext.GetOpenIddictServerRequest()
                      ?? throw new InvalidOperationException("OpenID Connect request cannot be retrieved.");

        var application =
            await _applicationManager.FindByClientIdAsync(request.ClientId ?? throw new InvalidOperationException())
            ?? throw new InvalidOperationException("Client application not found.");

        var user = await _userManager.GetUserAsync(User);
        if (user is null) throw new InvalidOperationException("User not found.");
        var model = new ConsentDto
        {
            ApplicationName = await _applicationManager.GetDisplayNameAsync(application) ?? "Unknown Application",
            Scope = request.Scope,
            RequestedScopes = new List<ScopeDescription>()
        };

        foreach (var scope in request.GetScopes())
        {
            var resource = await _scopeManager.FindByNameAsync(scope);
            model.RequestedScopes.Add(resource != null
                ? new ScopeDescription
                {
                    Name = scope,
                    DisplayName = await _scopeManager.GetDisplayNameAsync(resource) ?? scope,
                    Description = await _scopeManager.GetDescriptionAsync(resource)
                }
                : new ScopeDescription
                {
                    Name = scope,
                    DisplayName = scope,
                    Description = GetDefaultScopeDescription(scope)
                });
        }

        return Ok(model);
    }

    [HttpPost("~/connect/consent")]
    [Authorize] // ← ADDED
    public async Task<IActionResult> PostConsent([FromBody] ConsentActionModel? input)
    {
        if (input == null) return BadRequest("Invalid request payload");

        var request = HttpContext.GetOpenIddictServerRequest()
                      ?? throw new InvalidOperationException("OpenID Connect request cannot be retrieved.");

        var user = await _userManager.GetUserAsync(User)
                   ?? throw new InvalidOperationException("User not found.");

        if (input.Action.ToLower() == "deny")
        {
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.AccessDenied,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user denied access."
                }!));
        }

        if (request.ClientId is null) throw new InvalidOperationException("Client application not found.");
        var application = await _applicationManager.FindByClientIdAsync(request.ClientId)
                          ?? throw new InvalidOperationException("Client application not found.");
        var userId = await _userManager.GetUserIdAsync(user);

        var principal = await _signInManager.CreateUserPrincipalAsync(user);
        principal.SetClaim(OpenIddictConstants.Claims.Subject, userId);

        if (!string.IsNullOrEmpty(user.UserName))
            principal.SetClaim(OpenIddictConstants.Claims.Name, user.UserName);

        if (!string.IsNullOrEmpty(user.Email))
            principal.SetClaim(OpenIddictConstants.Claims.Email, user.Email);

        // ADDED: Email verified claim
        if (user.EmailConfirmed)
            principal.SetClaim(OpenIddictConstants.Claims.EmailVerified, true);

        principal.SetScopes(request.GetScopes());

        var resources = await _scopeManager.ListResourcesAsync(principal.GetScopes())
            .ToListAsync();
        principal.SetResources(resources);

        var authorization = await _authorizationManager.CreateAsync(
            principal: principal,
            subject: userId,
            client: await _applicationManager.GetIdAsync(application) ?? string.Empty,
            type: OpenIddictConstants.AuthorizationTypes.Permanent,
            scopes: principal.GetScopes(),
            cancellationToken: HttpContext.RequestAborted);

        principal.SetAuthorizationId(await _authorizationManager.GetIdAsync(authorization));

        foreach (var claim in principal.Claims)
        {
            claim.SetDestinations(GetDestinations(claim));
        }

        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }


    [HttpGet("~/connect/authorize")]
    [HttpPost("~/connect/authorize")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Authorize()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
                      throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        var application = await _clientRepo.GetViaConditionAsync(c => c.ClientId == request.ClientId! && c.IsActive);
        if (application == null)
        {
            _logger.LogWarning("Authorization attempt for unknown or inactive client: {ClientId}", request.ClientId);
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidClient,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                        "The client application is not recognized."
                }!));
        }
       
        if (!string.IsNullOrEmpty(request.CodeChallenge))
        {
            var codeChallengeMethod = request.CodeChallengeMethod ?? OpenIddictConstants.CodeChallengeMethods.Plain;
            if (codeChallengeMethod != OpenIddictConstants.CodeChallengeMethods.Sha256)
            {
                _logger.LogWarning("Invalid code challenge method: {Method} for client: {ClientId}",
                    codeChallengeMethod, request.ClientId);
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] =
                            OpenIddictConstants.Errors.InvalidRequest,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "Only S256 code challenge method is supported."
                    }!));
            }
        }

        var requestedScopes = request.GetScopes().ToList();
        var allowedScopes = await _applicationManager.GetPermissionsAsync(application);
        var allowedScopeNames = allowedScopes
            .Where(p => p.StartsWith(OpenIddictConstants.Permissions.Prefixes.Scope))
            .Select(p => p.Substring(OpenIddictConstants.Permissions.Prefixes.Scope.Length))
            .ToHashSet();

        var unauthorizedScopes = requestedScopes.Except(allowedScopeNames).ToList();
        if (unauthorizedScopes.Any())
        {
            _logger.LogWarning("Unauthorized scopes requested: {Scopes} for client: {ClientId}",
                string.Join(", ", unauthorizedScopes), request.ClientId);
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidScope,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                        "One or more requested scopes are not allowed."
                }!));
        }

        var redirectUris = await _applicationManager.GetRedirectUrisAsync(application);
        if (request.RedirectUri != null && !redirectUris.Contains(request.RedirectUri))
        {
            _logger.LogWarning("Invalid redirect URI: {RedirectUri} for client: {ClientId}",
                request.RedirectUri, request.ClientId);
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidRequest,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                        "The specified redirect URI is not valid."
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
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is not logged in."
                    }!));
            }

            // Store the authorization request in session for after login
            var returnUrl = Request.PathBase + Request.Path + QueryString.Create(
                Request.HasFormContentType ? Request.Form.ToList() : Request.Query.ToList());

            _logger.LogInformation("User not authenticated, redirecting to login for client: {ClientId}",
                request.ClientId);

            return Challenge(
                authenticationSchemes: IdentityConstants.ApplicationScheme,
                properties: new AuthenticationProperties
                {
                    RedirectUri = returnUrl
                });
        }

        var user = await _userManager.GetUserAsync(result.Principal);
        if (user == null)
        {
            _logger.LogError("User not found after successful authentication");
            return Challenge(authenticationSchemes: IdentityConstants.ApplicationScheme);
        }

        var userId = await _userManager.GetUserIdAsync(user);

        // Check if user account is locked or disabled
        if (!await _signInManager.CanSignInAsync(user))
        {
            _logger.LogWarning("User {UserId} cannot sign in (account may be locked)", userId);
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.AccessDenied,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                        "Your account cannot be accessed at this time."
                }!));
        }

        var authorizations = await _authorizationManager.FindAsync(
            subject: userId,
            client: await _applicationManager.GetIdAsync(application),
            status: OpenIddictConstants.Statuses.Valid,
            type: OpenIddictConstants.AuthorizationTypes.Permanent,
            scopes: ImmutableArray.CreateRange(requestedScopes),
            cancellationToken: HttpContext.RequestAborted).ToListAsync();

        // Check for external consent requirement
        if (!authorizations.Any() &&
            await _applicationManager.HasConsentTypeAsync(application, OpenIddictConstants.ConsentTypes.External))
        {
            _logger.LogWarning("External consent required but not granted for user {UserId} and client {ClientId}",
                userId, request.ClientId);
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.ConsentRequired,
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

        // Add email verified claim
        if (user.EmailConfirmed)
        {
            principal.SetClaim(OpenIddictConstants.Claims.EmailVerified, true);
        }

        principal.SetScopes(requestedScopes);
        principal.SetResources(await _scopeManager.ListResourcesAsync(principal.GetScopes()).ToListAsync());

        // Handle authorization with proper consent check
        var authorization = authorizations.LastOrDefault();
        if (authorization == null)
        {
            var consentType = await _applicationManager.GetConsentTypeAsync(application);

            // UPDATED: Better consent handling for mobile/SPA clients
            if (consentType == OpenIddictConstants.ConsentTypes.Explicit)
            {
                _logger.LogInformation("Consent required for user {UserId} and client {ClientId}", userId,
                    request.ClientId);

                // For mobile/SPA clients, return consent_required error
                // Client will then:
                // 1. Call GET /connect/consent to retrieve consent details
                // 2. Show consent UI to user
                // 3. Call POST /connect/consent with user's decision
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] =
                            OpenIddictConstants.Errors.ConsentRequired,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "User consent is required. Call GET /connect/consent to retrieve consent details, then POST /connect/consent with user decision."
                    }!));
            }

            // Create authorization for implicit consent clients
            authorization = await _authorizationManager.CreateAsync(
                principal: principal,
                subject: userId,
                client: (await _applicationManager.GetIdAsync(application))!,
                type: OpenIddictConstants.AuthorizationTypes.Permanent,
                scopes: principal.GetScopes(),
                cancellationToken: HttpContext.RequestAborted);

            _logger.LogInformation("Created new authorization for user {UserId} and client {ClientId}", userId,
                request.ClientId);
        }

        principal.SetAuthorizationId(await _authorizationManager.GetIdAsync(authorization, HttpContext.RequestAborted));

        // Set claim destinations
        foreach (var claim in principal.Claims)
        {
            claim.SetDestinations(GetDestinations(claim));
        }

        _logger.LogInformation("User {UserId} authorized client {ClientId}", userId, request.ClientId);
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
            var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            if (result.Principal == null)
            {
                _logger.LogWarning("Invalid authorization code used");
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] =
                            OpenIddictConstants.Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "The authorization code is invalid or expired."
                    }!));
            }

            var userId = result.Principal.GetClaim(OpenIddictConstants.Claims.Subject);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogError("Authorization code missing subject claim");
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
                _logger.LogWarning("User {UserId} not found during token exchange", userId);
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
                _logger.LogWarning("Security stamp validation failed for user {UserId}", userId);
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] =
                            OpenIddictConstants.Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "The authorization code is no longer valid."
                    }!));
            }

            // Ensure the user is still allowed to sign in
            if (!await _signInManager.CanSignInAsync(user))
            {
                _logger.LogWarning("User {UserId} is no longer allowed to sign in", userId);
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] =
                            OpenIddictConstants.Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "The authorization code is no longer valid."
                    }!));
            }

            foreach (var claim in result.Principal.Claims)
            {
                claim.SetDestinations(GetDestinations(claim));
            }

            _logger.LogInformation("Token issued for user {UserId} via authorization code", userId);
            return SignIn(result.Principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        if (request.IsRefreshTokenGrantType())
        {
            var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            if (result.Principal == null)
            {
                _logger.LogWarning("Invalid refresh token used");
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] =
                            OpenIddictConstants.Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "The refresh token is invalid or expired."
                    }!));
            }

            var userId = result.Principal.GetClaim(OpenIddictConstants.Claims.Subject);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogError("Refresh token missing subject claim");
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
                _logger.LogWarning("User {UserId} not found during refresh token exchange", userId);
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
                _logger.LogWarning("Security stamp validation failed for user {UserId} during refresh", userId);
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] =
                            OpenIddictConstants.Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "The refresh token is no longer valid."
                    }!));
            }

            // Ensure the user is still allowed to sign in
            if (!await _signInManager.CanSignInAsync(user))
            {
                _logger.LogWarning("User {UserId} is no longer allowed to sign in during refresh", userId);
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] =
                            OpenIddictConstants.Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "The refresh token is no longer valid."
                    }!));
            }

            foreach (var claim in result.Principal.Claims)
            {
                claim.SetDestinations(GetDestinations(claim));
            }

            _logger.LogInformation("Token refreshed for user {UserId}", userId);
            return SignIn(result.Principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        if (request.IsClientCredentialsGrantType())
        {
            if (request.ClientId != null)
            {
                var application = await _applicationManager.FindByClientIdAsync(request.ClientId);
                if (application == null)
                {
                    _logger.LogWarning("Client credentials grant attempted for unknown client: {ClientId}",
                        request.ClientId);
                    return Forbid(
                        authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                        properties: new AuthenticationProperties(new Dictionary<string, string>
                        {
                            [OpenIddictServerAspNetCoreConstants.Properties.Error] =
                                OpenIddictConstants.Errors.InvalidClient,
                            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                                "The client application is not recognized."
                        }!));
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
                    _logger.LogWarning(
                        "Unauthorized scopes requested in client credentials: {Scopes} for client: {ClientId}",
                        string.Join(", ", unauthorizedScopes), request.ClientId);
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

                _logger.LogInformation("Token issued for client {ClientId} via client credentials", request.ClientId);
                return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }
        }

        _logger.LogWarning("Unsupported grant type: {GrantType}", request.GrantType);
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
            _logger.LogInformation("Revoking all tokens for user {UserId} during logout", userId);
            var tokenManager = HttpContext.RequestServices.GetRequiredService<IOpenIddictTokenManager>();
            await foreach (var token in tokenManager.FindBySubjectAsync(userId))
            {
                await tokenManager.TryRevokeAsync(token);
            }
        }

        // Sign out from Identity
        await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

        _logger.LogInformation("User {UserId} logged out", userId ?? "Unknown");

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
            _logger.LogWarning("Userinfo request with missing subject claim");
            return Challenge(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidToken,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                        "The access token is missing required claims."
                }!));
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            _logger.LogWarning("Userinfo request for non-existent user: {UserId}", userId);
            return Challenge(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidToken,
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
            // NEW: Add more profile claims
            claims[OpenIddictConstants.Claims.UpdatedAt] = user.LockoutEnd?.ToUnixTimeSeconds() ?? 0;
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

    private static string GetDefaultScopeDescription(string scope) =>
        scope switch
        {
            OpenIddictConstants.Scopes.OpenId => "Access your user identifier",
            OpenIddictConstants.Scopes.Email => "Access your email address",
            OpenIddictConstants.Scopes.Profile => "Access your profile information",
            OpenIddictConstants.Scopes.Roles => "Access your roles and permissions",
            OpenIddictConstants.Scopes.OfflineAccess => "Access your data while you're offline",
            _ => $"Access {scope} information"
        };

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
            case OpenIddictConstants.Claims.EmailVerified:
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