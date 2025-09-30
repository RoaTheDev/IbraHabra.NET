using IbraHabra.NET.Application.Dto.Response;
using IbraHabra.NET.Application.Services;
using IbraHabra.NET.Domain.Entity;
using IbraHabra.NET.Domain.SharedKernel.Interface;
using IbraHabra.NET.Domain.SharedKernel.Interface.Services;
using Microsoft.AspNetCore.Identity;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Users.Commands.LoginUser;

public record LoginUserCommand(string Email, string Password, string ClientId);

public record LoginUserCommandResponse(
    Guid? UserId = null,
    bool RequiresTwoFactor = false,
    string? TwoFactorToken = null);

public class LoginUserHandler : IWolverineHandler
{
    public static async Task<ApiResult<LoginUserCommandResponse>> Handle(
        LoginUserCommand command,
        IRepo<OauthApplication, string> repo,
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        ITwoFactorTokenService tokenService)
    {
        var client = await repo.GetViaConditionAsync(c => c.ClientId == command.ClientId && c.IsActive);
        if (client == null)
            return ApiResult<LoginUserCommandResponse>.Fail(400, "Invalid client.");

        var user = await userManager.FindByEmailAsync(command.Email);
        if (user == null)
            return ApiResult<LoginUserCommandResponse>.Fail(401, "Invalid credentials.");

        var policy = client.GetAuthPolicy();

        if (policy.RequireEmailVerification && !user.EmailConfirmed)
            return ApiResult<LoginUserCommandResponse>.Fail(401, "Email not verified.");

        var result = await signInManager.CheckPasswordSignInAsync(user, command.Password, true);
        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
                return ApiResult<LoginUserCommandResponse>.Fail(423, "Account locked due to too many failed attempts.");

            return ApiResult<LoginUserCommandResponse>.Fail(401, "Invalid credentials.");
        }

        if (result.RequiresTwoFactor)
        {
            var twoFactorToken = await tokenService.CreateTokenAsync(user.Id, command.ClientId);

            return ApiResult<LoginUserCommandResponse>.Ok(202, new(
                UserId: null,
                RequiresTwoFactor: true,
                TwoFactorToken: twoFactorToken));
        }

        if (policy.RequireMfa && !await userManager.GetTwoFactorEnabledAsync(user))
            return ApiResult<LoginUserCommandResponse>.Fail(401,
                "This application now requires two-factor authentication. Please set up 2FA to continue.");

        await signInManager.SignInAsync(user, isPersistent: false);

        return ApiResult<LoginUserCommandResponse>.Ok(new(user.Id));
    }
}