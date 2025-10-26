using System.Security.Claims;
using IbraHabra.NET.Application.Dto;
using IbraHabra.NET.Domain.Constants;
using IbraHabra.NET.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Admin.Commands.Disable2FaAdmin;

public record Disable2FaAdminRequest(string Password);

public record Disable2FaAdminCommand(string Password, HttpContext HttpContext);

public record Disable2FaAdminResponse(
    bool Success,
    string Message);

public class Disable2FaAdminHandler : IWolverineHandler
{
    public static async Task<ApiResult<Disable2FaAdminResponse>> Handle(
        Disable2FaAdminCommand command,
        UserManager<User> userManager)
    {
        var userId = command.HttpContext?.User?.FindFirst("sub")?.Value
                     ?? command.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return ApiResult<Disable2FaAdminResponse>.Fail(ApiErrors.Authentication.InvalidToken());

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
            return ApiResult<Disable2FaAdminResponse>.Fail(ApiErrors.User.NotFound());

        var passwordValid = await userManager.CheckPasswordAsync(user, command.Password);
        if (!passwordValid)
            return ApiResult<Disable2FaAdminResponse>.Fail(ApiErrors.Authentication.InvalidCredentials());

        // Check if 2FA is enabled
        var isTwoFactorEnabled = await userManager.GetTwoFactorEnabledAsync(user);
        if (!isTwoFactorEnabled)
            return ApiResult<Disable2FaAdminResponse>.Fail(ApiErrors.User.CannotDisableTwoFactor());

        // Disable 2FA
        var disable2FaResult = await userManager.SetTwoFactorEnabledAsync(user, false);
        if (!disable2FaResult.Succeeded)
        {
            return ApiResult<Disable2FaAdminResponse>.Fail(ApiErrors.User.FailToDisableTwoFactor());
        }

        // Reset the authenticator key
        await userManager.ResetAuthenticatorKeyAsync(user);

        return ApiResult<Disable2FaAdminResponse>.Ok(new Disable2FaAdminResponse(
            Success: true,
            Message: "Two-factor authentication has been disabled successfully"));
    }
}