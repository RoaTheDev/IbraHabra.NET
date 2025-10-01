using System.Security.Claims;
using IbraHabra.NET.Application.Dto.Response;
using IbraHabra.NET.Application.Utils;
using IbraHabra.NET.Domain.Entity;
using IbraHabra.NET.Domain.SharedKernel.Interface;
using Microsoft.AspNetCore.Identity;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Users.Commands;

public record Initialize2FaSetupCommand(ClaimsPrincipal Claims, string ClientId);

public record Setup2FaInfoResponse(
    string QrCodeUri,
    string ManualEntryKey);

public class Initialize2FaSetupHandler : IWolverineHandler
{
    public static async Task<ApiResult<Setup2FaInfoResponse>> Handle(
        Initialize2FaSetupCommand command,
        UserManager<User> userManager,
        IRepo<OauthApplication, string> repo)
    {
        var client = await repo.GetViaConditionAsync(c => c.ClientId == command.ClientId && c.IsActive,
            c => new { c.DisplayName });

        if (client is null)
            return ApiResult<Setup2FaInfoResponse>.Fail(404, "Client does not exist");

        var user = await userManager.GetUserAsync(command.Claims);

        if (user is null)
            return ApiResult<Setup2FaInfoResponse>.Fail(401, "Not authenticated.");

        if (await userManager.GetTwoFactorEnabledAsync(user))
            return ApiResult<Setup2FaInfoResponse>.Fail(400, "2FA is already enabled. Disable it first to reset.");

        await userManager.ResetAuthenticatorKeyAsync(user);
        var key = await userManager.GetAuthenticatorKeyAsync(user);

        if (string.IsNullOrEmpty(key))
            return ApiResult<Setup2FaInfoResponse>.Fail(500, "Failed to generate authenticator key.");

        var email = user.Email!;
        var qrCodeUri = TwoFactorUtils.GenerateQrCodeUri(email, key, client.DisplayName!);

        return ApiResult<Setup2FaInfoResponse>.Ok(new(
            QrCodeUri: qrCodeUri,
            ManualEntryKey: TwoFactorUtils.FormatKey(key)));
    }
}