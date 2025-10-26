using System.Security.Claims;
using System.Text;
using IbraHabra.NET.Application.Dto;
using IbraHabra.NET.Domain.Constants;
using IbraHabra.NET.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Admin.Commands.Enable2FaAdmin;

public record Enable2FaAdminCommand(HttpContext HttpContext);

public record Enable2FaAdminResponse(
    string SharedKey,
    string AuthenticatorUri,
    string[] RecoveryCodes);

public class Enable2FaAdminHandler : IWolverineHandler
{
    public static async Task<ApiResult<Enable2FaAdminResponse>> Handle(
        Enable2FaAdminCommand command,
        UserManager<User> userManager)
    {
        var userId = command.HttpContext?.User?.FindFirst("sub")?.Value
                     ?? command.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return ApiResult<Enable2FaAdminResponse>.Fail(ApiErrors.Authentication.InvalidToken());

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
            return ApiResult<Enable2FaAdminResponse>.Fail(ApiErrors.User.NotFound());

        var isTwoFactorEnabled = await userManager.GetTwoFactorEnabledAsync(user);
        if (isTwoFactorEnabled)
            return ApiResult<Enable2FaAdminResponse>.Fail(ApiErrors.User.CannotEnableTwoFactor());

        await userManager.ResetAuthenticatorKeyAsync(user);
        var unformattedKey = await userManager.GetAuthenticatorKeyAsync(user);

        if (string.IsNullOrEmpty(unformattedKey))
            return ApiResult<Enable2FaAdminResponse>.Fail(ApiErrors.User.FailAuthKeyGeneration());

        var sharedKey = FormatKey(unformattedKey);

        var email = await userManager.GetEmailAsync(user);
        var authenticatorUri = GenerateQrCodeUri(email!, unformattedKey);

        var recoveryCodes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

        return ApiResult<Enable2FaAdminResponse>.Ok(new Enable2FaAdminResponse(
            SharedKey: sharedKey,
            AuthenticatorUri: authenticatorUri,
            RecoveryCodes: recoveryCodes?.ToArray() ?? Array.Empty<string>()));
    }

    private static string FormatKey(string unformattedKey)
    {
        var result = new StringBuilder();
        int currentPosition = 0;
        while (currentPosition + 4 < unformattedKey.Length)
        {
            result.Append(unformattedKey.AsSpan(currentPosition, 4)).Append(' ');
            currentPosition += 4;
        }

        if (currentPosition < unformattedKey.Length)
        {
            result.Append(unformattedKey.AsSpan(currentPosition));
        }

        return result.ToString().ToLowerInvariant();
    }

    private static string GenerateQrCodeUri(string email, string unformattedKey)
    {
        const string authenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";

        return string.Format(
            authenticatorUriFormat,
            Uri.EscapeDataString("IbraHabra.NET"),
            Uri.EscapeDataString(email),
            unformattedKey);
    }
}