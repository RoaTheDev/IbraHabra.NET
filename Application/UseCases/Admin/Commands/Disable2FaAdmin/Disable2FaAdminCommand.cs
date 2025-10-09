using IbraHabra.NET.Application.Dto.Response;
using IbraHabra.NET.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Admin.Commands.Disable2FaAdmin;

public record Disable2FaAdminCommand(string Password);

public record Disable2FaAdminResponse(
    bool Success,
    string Message);

public class Disable2FaAdminHandler : IWolverineHandler
{
    public static async Task<ApiResult<Disable2FaAdminResponse>> Handle(
        Disable2FaAdminCommand command,
        UserManager<User> userManager,
        IHttpContextAccessor httpContextAccessor)
    {
        var userId = httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value 
            ?? httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return ApiResult<Disable2FaAdminResponse>.Fail(401, "Unauthorized");

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
            return ApiResult<Disable2FaAdminResponse>.Fail(404, "User not found");

        // Verify password for security
        var passwordValid = await userManager.CheckPasswordAsync(user, command.Password);
        if (!passwordValid)
            return ApiResult<Disable2FaAdminResponse>.Fail(401, "Invalid password");

        // Check if 2FA is enabled
        var isTwoFactorEnabled = await userManager.GetTwoFactorEnabledAsync(user);
        if (!isTwoFactorEnabled)
            return ApiResult<Disable2FaAdminResponse>.Fail(400, "2FA is not enabled");

        // Disable 2FA
        var disable2FaResult = await userManager.SetTwoFactorEnabledAsync(user, false);
        if (!disable2FaResult.Succeeded)
        {
            return ApiResult<Disable2FaAdminResponse>.Fail(500, 
                "Failed to disable 2FA: " + string.Join(", ", disable2FaResult.Errors.Select(e => e.Description)));
        }

        // Reset the authenticator key
        await userManager.ResetAuthenticatorKeyAsync(user);

        return ApiResult<Disable2FaAdminResponse>.Ok(new Disable2FaAdminResponse(
            Success: true,
            Message: "Two-factor authentication has been disabled successfully"));
    }
}