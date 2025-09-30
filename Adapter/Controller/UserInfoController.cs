using System.Security.Claims;
using IbraHabra.NET.Domain.Entity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace IbraHabra.NET.Adapter.Controller;

[ApiController]
public class UserInfoController : ControllerBase
{
    private readonly UserManager<User> _userManager;

    public UserInfoController(UserManager<User> userManager)
        => _userManager = userManager;

    [Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
    [HttpGet("~/connect/userinfo", Name = "UserInfoEndpoint"), HttpPost("~/connect/userinfo")]
    public async Task<IActionResult> UserInfo()
    {
        var user = await _userManager.FindByIdAsync(User.GetClaim(ClaimTypes.NameIdentifier)!);
        if (user is null)
        {
            return Challenge(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidToken,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                        "The specified access token is bound to an account that no longer exists."
                }!));
        }

        var claims = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            [OpenIddictConstants.Claims.Subject] = await _userManager.GetUserIdAsync(user)
        };

        if (User.HasScope(OpenIddictConstants.Permissions.Scopes.Email))
        {
            claims[OpenIddictConstants.Claims.Email] = await _userManager.GetEmailAsync(user) ?? "";
            claims[OpenIddictConstants.Claims.EmailVerified] = await _userManager.IsEmailConfirmedAsync(user);
        }

        if (User.HasScope(OpenIddictConstants.Permissions.Scopes.Profile))
        {
            claims[OpenIddictConstants.Claims.Name] = $"{user.FirstName} {user.LastName}".Trim();
            claims[OpenIddictConstants.Claims.GivenName] = user.FirstName ?? "";
            claims[OpenIddictConstants.Claims.FamilyName] = user.LastName ?? "";
        }

        if (User.HasScope(OpenIddictConstants.Permissions.Scopes.Roles))
        {
            claims[OpenIddictConstants.Claims.Role] = await _userManager.GetRolesAsync(user);
        }

        return Ok(claims);
    }
    
}