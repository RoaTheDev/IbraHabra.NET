using IbraHabra.NET.Application.Dto.Response;
using IbraHabra.NET.Application.Helper;
using IbraHabra.NET.Domain.Entity;
using IbraHabra.NET.Domain.Interface;
using Microsoft.AspNetCore.Identity;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Users;

public record LoginUserCommand(string Email, string Password, string ClientId);

public record LoginUserCommandResponse(Guid Id);

public class LoginUserHandler : IWolverineHandler
{
    public static async Task<ApiResult<LoginUserCommandResponse>> Handle(
        LoginUserCommand command,
        IRepo<OauthApplication, string> repo,
        UserManager<User> userManager,
        SignInManager<User> signInManager)
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

        if (policy.RequireMfa && !await userManager.GetTwoFactorEnabledAsync(user))
            return ApiResult<LoginUserCommandResponse>.Fail(401, "MFA required.");

        var result = await signInManager.CheckPasswordSignInAsync(user, command.Password, true);
        if (!result.Succeeded)
            return ApiResult<LoginUserCommandResponse>.Fail(401, "Invalid credentials.");

        await signInManager.SignInAsync(user, isPersistent: false);
        
        return ApiResult<LoginUserCommandResponse>.Ok(new(user.Id));
    }
}