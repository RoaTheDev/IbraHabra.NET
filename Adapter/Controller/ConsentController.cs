using System.Security.Claims;
using IbraHabra.NET.Application.Dto.Request.User;
using IbraHabra.NET.Domain.Entities;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace IbraHabra.NET.Adapter.Controller;

[Authorize]
[ApiController]
[Route("connect")]
public class ConsentController : ControllerBase
{
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IOpenIddictAuthorizationManager _authorizationManager;
    private readonly IOpenIddictScopeManager _scopeManager;
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;

    public ConsentController(
        IOpenIddictApplicationManager applicationManager,
        IOpenIddictAuthorizationManager authorizationManager,
        IOpenIddictScopeManager scopeManager,
        UserManager<User> userManager,
        SignInManager<User> signInManager)
    {
        _applicationManager = applicationManager;
        _authorizationManager = authorizationManager;
        _scopeManager = scopeManager;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    // GET: /connect/consent - return JSON for SPA/mobile
    [HttpGet("consent")]
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

    [HttpPost("consent")]
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

        // User accepted consent
        if (request.ClientId is null) throw new InvalidOperationException("Client application not found.");
        var application = await _applicationManager.FindByClientIdAsync(request.ClientId);
        if (application is null) throw new InvalidOperationException("Client application not found.");
        var userId = await _userManager.GetUserIdAsync(user);

        var principal = await _signInManager.CreateUserPrincipalAsync(user);
        principal.SetClaim(OpenIddictConstants.Claims.Subject, userId);

        if (!string.IsNullOrEmpty(user.UserName))
            principal.SetClaim(OpenIddictConstants.Claims.Name, user.UserName);

        if (!string.IsNullOrEmpty(user.Email))
            principal.SetClaim(OpenIddictConstants.Claims.Email, user.Email);

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

    private static IEnumerable<string> GetDestinations(Claim claim)
    {
        switch (claim.Type)
        {
            case OpenIddictConstants.Claims.Subject:
                yield return OpenIddictConstants.Destinations.AccessToken;
                yield return OpenIddictConstants.Destinations.IdentityToken;
                yield break;
            case OpenIddictConstants.Claims.Name:
                yield return OpenIddictConstants.Destinations.AccessToken;
                yield return OpenIddictConstants.Destinations.IdentityToken;
                yield break;
            case OpenIddictConstants.Claims.Email:
                yield return OpenIddictConstants.Destinations.AccessToken;
                yield return OpenIddictConstants.Destinations.IdentityToken;
                yield break;
            case OpenIddictConstants.Claims.Role:
                yield return OpenIddictConstants.Destinations.AccessToken;
                yield return OpenIddictConstants.Destinations.IdentityToken;
                yield break;
            case "AspNet.Identity.SecurityStamp":
                yield break;
            default:
                yield return OpenIddictConstants.Destinations.AccessToken;
                yield break;
        }
    }
}


