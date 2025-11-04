using IbraHabra.NET.Application.Dto;
using IbraHabra.NET.Application.Utils;
using IbraHabra.NET.Domain.Constants;
using IbraHabra.NET.Domain.Contract;
using IbraHabra.NET.Domain.Contract.Services;
using IbraHabra.NET.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Admin.Commands.Verify2FaAdmin;

public record Verify2FaAdminCommand(string Session2Fa, string Code);

public record Verify2FaAdminCommandResponse(
    Guid UserId,
    string Email,
    string Token,
    DateTime ExpiresAt);

public class Verify2FaAdminHandler : IWolverineHandler
{
    public static async Task<ApiResult<Verify2FaAdminCommandResponse>> Handle(
        Verify2FaAdminCommand command,
        UserManager<User> userManager,
        ICacheService cache,
        IOptions<JwtOptions> jwtOptions,
        IHttpContextAccessor httpContextAccessor,
        IRefreshTokenService refreshTokenService)
    {
        var cachedEmail = await cache.GetAsync<string>($"2fa:{command.Session2Fa}");
        await cache.RemoveAsync($"2fa:{command.Session2Fa}");
        if (cachedEmail == null)
            return ApiResult<Verify2FaAdminCommandResponse>.Fail(ApiErrors.Authentication.InvalidSession());

        var user = await userManager.FindByEmailAsync(cachedEmail);
        if (user == null)
            return ApiResult<Verify2FaAdminCommandResponse>.Fail(ApiErrors.User.NotFound());

        var roles = await userManager.GetRolesAsync(user);
        if (!roles.Contains("Admin") && !roles.Contains("Super"))
            return ApiResult<Verify2FaAdminCommandResponse>.Fail(ApiErrors.Authorization.InsufficientPermissions());

        var isValid = await userManager.VerifyTwoFactorTokenAsync(
            user,
            userManager.Options.Tokens.AuthenticatorTokenProvider,
            command.Code);

        if (!isValid)
        {
            await userManager.AccessFailedAsync(user);

            if (await userManager.IsLockedOutAsync(user))
            {
                await cache.RemoveAsync($"2fa:{command.Session2Fa}");
                var lockoutEnd = await userManager.GetLockoutEndDateAsync(user);
                var remainingMinutes = (lockoutEnd - DateTimeOffset.UtcNow)?.TotalMinutes ?? 0;
                return ApiResult<Verify2FaAdminCommandResponse>.Fail(
                    ApiErrors.User.AccountLocked(Convert.ToInt32(remainingMinutes)));
            }

            return ApiResult<Verify2FaAdminCommandResponse>.Fail(ApiErrors.User.InvalidTwoFactorCode());
        }

        await cache.RemoveAsync($"2fa:{command.Session2Fa}");
        await userManager.ResetAccessFailedCountAsync(user);

        var token = await JwtGen.GenerateJwtToken(user, userManager, jwtOptions);
        var expiresAt = DateTime.UtcNow.AddHours(8);
        var refreshToken = await refreshTokenService.GenerateAndStoreAsync(user.Id);
        refreshTokenService.SetRefreshTokenCookie(httpContextAccessor.HttpContext!, refreshToken);

        return ApiResult<Verify2FaAdminCommandResponse>.Ok(new Verify2FaAdminCommandResponse(
            UserId: user.Id,
            Email: user.Email!,
            Token: token,
            ExpiresAt: expiresAt));
    }
}