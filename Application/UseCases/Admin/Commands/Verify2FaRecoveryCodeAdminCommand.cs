using IbraHabra.NET.Application.Dto;
using IbraHabra.NET.Application.Utils;
using IbraHabra.NET.Domain.Constants;
using IbraHabra.NET.Domain.Contract;
using IbraHabra.NET.Domain.Contract.Services;
using IbraHabra.NET.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Admin.Commands;

public record VerifyRecoveryCodeAdminCommand(string Session2Fa, string RecoveryCode);

public record VerifyRecoveryCodeAdminResponse(
    Guid UserId,
    string Email,
    string Token,
    DateTime ExpiresAt,
    int RemainingRecoveryCodes);

public class VerifyRecoveryCodeAdminHandler : IWolverineHandler
{
    public static async Task<ApiResult<VerifyRecoveryCodeAdminResponse>> Handle(
        VerifyRecoveryCodeAdminCommand command,
        UserManager<User> userManager,
        ICacheService cache,
        IOptions<JwtOptions> jwtOptions,
        IHttpContextAccessor httpContextAccessor,
        IRefreshTokenService refreshTokenService)
    {
        var cachedEmail = await cache.GetAsync<string>($"2fa:{command.Session2Fa}");

        if (cachedEmail == null)
            return ApiResult<VerifyRecoveryCodeAdminResponse>.Fail(
                ApiErrors.Authentication.InvalidSession());

        var user = await userManager.FindByEmailAsync(cachedEmail);
        if (user == null)
        {
            await cache.RemoveAsync($"2fa:{command.Session2Fa}");
            return ApiResult<VerifyRecoveryCodeAdminResponse>.Fail(
                ApiErrors.User.NotFound());
        }

        var roles = await userManager.GetRolesAsync(user);
        if (!roles.Contains("Admin") && !roles.Contains("SuperAdmin"))
        {
            await cache.RemoveAsync($"2fa:{command.Session2Fa}");
            return ApiResult<VerifyRecoveryCodeAdminResponse>.Fail(
                ApiErrors.Authorization.InsufficientPermissions());
        }

        var result = await userManager.RedeemTwoFactorRecoveryCodeAsync(user, command.RecoveryCode);
        if (!result.Succeeded)
        {
            await userManager.AccessFailedAsync(user);

            if (await userManager.IsLockedOutAsync(user))
            {
                await cache.RemoveAsync($"2fa:{command.Session2Fa}");
                var lockoutEnd = await userManager.GetLockoutEndDateAsync(user);
                var remainingMinutes = (lockoutEnd - DateTimeOffset.UtcNow)?.TotalMinutes ?? 0;
                return ApiResult<VerifyRecoveryCodeAdminResponse>.Fail(
                    ApiErrors.User.AccountLocked(Convert.ToInt32(remainingMinutes)));
            }

            return ApiResult<VerifyRecoveryCodeAdminResponse>.Fail(
                ApiErrors.User.InvalidTwoFactorCode());
        }

        await cache.RemoveAsync($"2fa:{command.Session2Fa}");

        await userManager.ResetAccessFailedCountAsync(user);

        var token = await JwtGen.GenerateJwtToken(user, userManager, jwtOptions);
        var expiresAt = DateTime.UtcNow.AddHours(8);
        var refreshToken = await refreshTokenService.GenerateAndStoreAsync(user.Id);
        refreshTokenService.SetRefreshTokenCookie(httpContextAccessor.HttpContext!, refreshToken);

        var remainingCodes = await userManager.CountRecoveryCodesAsync(user);

        return ApiResult<VerifyRecoveryCodeAdminResponse>.Ok(
            new VerifyRecoveryCodeAdminResponse(
                UserId: user.Id,
                Email: user.Email!,
                Token: token,
                ExpiresAt: expiresAt,
                RemainingRecoveryCodes: remainingCodes));
    }
}