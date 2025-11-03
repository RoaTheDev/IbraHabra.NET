using System.Security.Claims;
using IbraHabra.NET.Application.Dto;
using IbraHabra.NET.Domain.Constants;
using IbraHabra.NET.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Admin.Commands;

public record RegenerateRecoveryCodesAdminCommand(string Password, HttpContext HttpContext);

public record RegenerateRecoveryCodesAdminRequest(string Password);

public record RegenerateRecoveryCodesAdminResponse(
    string[] RecoveryCodes,
    string Message);

public class RegenerateRecoveryCodesAdminHandler : IWolverineHandler
{
    public static async Task<ApiResult<RegenerateRecoveryCodesAdminResponse>> Handle(
        RegenerateRecoveryCodesAdminCommand command,
        UserManager<User> userManager)
    {
        var userId = command.HttpContext.User.FindFirst("sub")?.Value
                     ?? command.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return ApiResult<RegenerateRecoveryCodesAdminResponse>.Fail(
                ApiErrors.Authentication.InvalidToken());

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return ApiResult<RegenerateRecoveryCodesAdminResponse>.Fail(
                ApiErrors.User.NotFound());

        var passwordValid = await userManager.CheckPasswordAsync(user, command.Password);
        if (!passwordValid)
            return ApiResult<RegenerateRecoveryCodesAdminResponse>.Fail(
                ApiErrors.Authentication.InvalidCredentials());

        var isTwoFactorEnabled = await userManager.GetTwoFactorEnabledAsync(user);
        if (!isTwoFactorEnabled)
            return ApiResult<RegenerateRecoveryCodesAdminResponse>.Fail(
                ApiErrors.User.Regenerate2FaRecoveryCode());

        var recoveryCodesEnumerable = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
        var recoveryCodes = recoveryCodesEnumerable?.ToArray();

        if (recoveryCodes is null || recoveryCodes.Length == 0)
            return ApiResult<RegenerateRecoveryCodesAdminResponse>.Fail(
                ApiErrors.Common.InvalidRequest());

        return ApiResult<RegenerateRecoveryCodesAdminResponse>.Ok(
            new RegenerateRecoveryCodesAdminResponse(
                RecoveryCodes: recoveryCodes,
                Message: "New recovery codes generated successfully. All previous codes are now invalid."));
    }
}