using IbraHabra.NET.Application.Dto;
using IbraHabra.NET.Application.Utils;
using IbraHabra.NET.Domain.Constants;
using IbraHabra.NET.Domain.Contract;
using IbraHabra.NET.Domain.Contract.Services;
using IbraHabra.NET.Domain.Entities;
using IbraHabra.NET.Infra.Persistent.Projections;
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
        var client = await repo.GetViaConditionAsync(c => c.ClientId == command.ClientId && c.IsActive, c =>
            new AuthPolicyProjections(c.Properties));

        if (client is null)
            return ApiResult<LoginUserCommandResponse>.Fail(ApiErrors.OAuthApplication.NotFound());

        var user = await userManager.FindByEmailAsync(command.Email);
        if (user == null)
            return ApiResult<LoginUserCommandResponse>.Fail(ApiErrors.Authentication.InvalidCredentials());

        var policy = ReadAuthPolicy.GetAuthPolicy(client.Properties);

        // var passwordValidationRes = ReadAuthPolicy.ValidatePasswordAgainstPolicy(command.Password, policy);
        // if (!passwordValidationRes.isPassed)
        //     return ApiResult<LoginUserCommandResponse>.Fail(ApiErrors.Authentication.InvalidCredentials());

        if (policy.RequireEmailVerification && !user.EmailConfirmed)
            return ApiResult<LoginUserCommandResponse>.Fail(ApiErrors.User.EmailNotVerified());

        var result = await signInManager.CheckPasswordSignInAsync(user, command.Password, true);
        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
            {
                var lockoutMinutes = userManager.Options.Lockout.DefaultLockoutTimeSpan.TotalMinutes;
                return ApiResult<LoginUserCommandResponse>.Fail(
                    ApiErrors.User.AccountLocked(Convert.ToInt32(lockoutMinutes)));
            }

            return ApiResult<LoginUserCommandResponse>.Fail(ApiErrors.Authentication.InvalidCredentials());
        }

        if (result.RequiresTwoFactor)
        {
            var twoFactorToken = await tokenService.CreateTokenAsync(user.Id, command.ClientId);

            return ApiResult<LoginUserCommandResponse>.Ok(new(
                UserId: null,
                RequiresTwoFactor: true,
                TwoFactorToken: twoFactorToken));
        }

        if (policy.RequireMfa && !await userManager.GetTwoFactorEnabledAsync(user))
            return ApiResult<LoginUserCommandResponse>.Fail(ApiErrors.Authentication.TwoFactorRequired());

        await signInManager.SignInAsync(user, isPersistent: false);

        return ApiResult<LoginUserCommandResponse>.Ok(new(user.Id));
    }
}