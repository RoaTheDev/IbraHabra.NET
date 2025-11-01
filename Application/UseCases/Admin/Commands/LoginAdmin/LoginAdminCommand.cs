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
    string Token,
    DateTime ExpiresAt,
    bool RequiresTwoFactor = false,
    string? Session2Fa = null);

public class LoginAdminHandler : IWolverineHandler
{
    public static async Task<ApiResult<LoginAdminCommandResponse>> Handle(
        LoginAdminCommand command,
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        ICacheService cache,
        IOptions<JwtOptions> jwtOptions,
        IHttpContextAccessor httpContextAccessor,
        IRefreshTokenService refreshTokenService
    )
    {
        var user = await userManager.FindByEmailAsync(command.Email);
        if (user == null)
            return ApiResult<LoginAdminCommandResponse>.Fail(ApiErrors.User.NotFound());

        var roles = await userManager.GetRolesAsync(user);
        Console.WriteLine(string.Join(", ", roles));
        if (!roles.Contains("Admin") && !roles.Contains("Super"))
            return ApiResult<LoginAdminCommandResponse>.Fail(ApiErrors.Authorization.InsufficientPermissions());

        var result = await signInManager.CheckPasswordSignInAsync(user, command.Password, true);
        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
            {
                var lockoutMinutes = userManager.Options.Lockout.DefaultLockoutTimeSpan.TotalMinutes;
                return ApiResult<LoginAdminCommandResponse>.Fail(
                    ApiErrors.User.AccountLocked(Convert.ToInt32(lockoutMinutes)));
            }

            return ApiResult<LoginAdminCommandResponse>.Fail(ApiErrors.Authentication.InvalidCredentials());
        }

        if (result.RequiresTwoFactor)
        {
            var session2Fa = Guid.NewGuid().ToString();
            await cache.SetAsync($"2fa:{session2Fa}", user.Email, TimeSpan.FromMinutes(5));

            return ApiResult<LoginAdminCommandResponse>.Ok(new LoginAdminCommandResponse(
                UserId: Guid.Empty,
                Email: "default@ibrahara.com",
                Token: string.Empty,
                ExpiresAt: DateTime.UtcNow,
                RequiresTwoFactor: true,
                Session2Fa: session2Fa));
        }

        var token = await JwtGen.GenerateJwtToken(user, userManager, jwtOptions);
        var expiresAt = DateTime.UtcNow.AddHours(1);

        var refreshToken = await refreshTokenService.GenerateAndStoreAsync(user.Id);
        refreshTokenService.SetRefreshTokenCookie(httpContextAccessor.HttpContext!, refreshToken);

        return ApiResult<LoginAdminCommandResponse>.Ok(new LoginAdminCommandResponse(
            UserId: user.Id,
            Email: user.Email!,
            Token: token,
            ExpiresAt: expiresAt));
    }
}