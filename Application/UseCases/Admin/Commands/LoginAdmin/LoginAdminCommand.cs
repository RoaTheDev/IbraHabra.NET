using IbraHabra.NET.Application.Dto;
using IbraHabra.NET.Application.Utils;
using IbraHabra.NET.Domain.Constants;
using IbraHabra.NET.Domain.Contract;
using IbraHabra.NET.Domain.Contract.Services;
using IbraHabra.NET.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Admin.Commands.LoginAdmin;

public record LoginAdminCommand(string Email, string Password);

public record LoginAdminCommandResponse(
    Guid UserId,
    string Email,
    bool RequiresTwoFactor = false,
    string? Session2Fa = null);

public class LoginAdminHandler : IWolverineHandler
{
    public static async Task<ApiResult<LoginAdminCommandResponse>> Handle(
        LoginAdminCommand command,
        UserManager<User> userManager,
        ICacheService cache,
        IOptions<JwtOptions> jwtOptions,
        IHttpContextAccessor httpContextAccessor,
        ITokenService tokenService)
    {
        var user = await userManager.FindByEmailAsync(command.Email);
        if (user == null)
            return ApiResult<LoginAdminCommandResponse>.Fail(ApiErrors.User.NotFound());

        var roles = await userManager.GetRolesAsync(user);
        if (!roles.Contains("Admin") && !roles.Contains("Super"))
            return ApiResult<LoginAdminCommandResponse>.Fail(
                ApiErrors.Authorization.InsufficientPermissions());

        if (!await userManager.CheckPasswordAsync(user, command.Password))
        {
            await userManager.AccessFailedAsync(user);

            if (await userManager.IsLockedOutAsync(user))
            {
                var lockoutEnd = await userManager.GetLockoutEndDateAsync(user);
                var remainingMinutes = (lockoutEnd - DateTimeOffset.UtcNow)?.TotalMinutes ?? 0;
                return ApiResult<LoginAdminCommandResponse>.Fail(
                    ApiErrors.User.AccountLocked(Convert.ToInt32(remainingMinutes)));
            }

            return ApiResult<LoginAdminCommandResponse>.Fail(
                ApiErrors.Authentication.InvalidCredentials());
        }

        await userManager.ResetAccessFailedCountAsync(user);

        if (user.TwoFactorEnabled)
        {
            var session2Fa = Guid.NewGuid().ToString();
            await cache.SetAsync($"2fa:{session2Fa}", user.Email, TimeSpan.FromMinutes(5));

            return ApiResult<LoginAdminCommandResponse>.Ok(new LoginAdminCommandResponse(
                UserId: Guid.Empty,
                Email: "default@ibrahara.com",
                RequiresTwoFactor: true,
                Session2Fa: session2Fa));
        }

        var token = await JwtGen.GenerateJwtToken(user, userManager, jwtOptions);
        var expiresAt = DateTime.UtcNow.AddHours(8);

        var refreshToken = await tokenService.GenerateAndStoreAsync(user.Id);
        tokenService.SetAccessTokenCookie(httpContextAccessor.HttpContext!, token);
        tokenService.SetRefreshTokenCookie(httpContextAccessor.HttpContext!, refreshToken);

        return ApiResult<LoginAdminCommandResponse>.Ok(new LoginAdminCommandResponse(
            UserId: user.Id,
            Email: user.Email!));
    }
}