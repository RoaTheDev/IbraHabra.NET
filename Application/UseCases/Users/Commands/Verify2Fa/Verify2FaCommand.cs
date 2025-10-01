using IbraHabra.NET.Application.Dto.Response;
using IbraHabra.NET.Domain.Entity;
using IbraHabra.NET.Domain.SharedKernel.Interface;
using IbraHabra.NET.Domain.SharedKernel.Interface.Services;
using Microsoft.AspNetCore.Identity;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Users.Commands.Verify2Fa;

public record Verify2FaCommand(string TwoFactorToken, string Totp, string ClientId);

public record Verify2FaResponse(Guid Id);

public class Verify2FaHandler : IWolverineHandler
{
    public static async Task<ApiResult<Verify2FaResponse>> Handle(Verify2FaCommand command,
        UserManager<User> userManager, SignInManager<User> signInManager, IRepo<OauthApplication, string> repo,
        ITwoFactorTokenService tokenService)
    {
        var client = await repo.GetViaConditionAsync(c => c.ClientId == command.ClientId && c.IsActive,
            c => new { c.ClientId });
        if (client == null)
            return ApiResult<Verify2FaResponse>.Fail(400, "Invalid client.");

        var tokenData = await tokenService.ValidateAndRemoveTokenAsync(command.TwoFactorToken);
        if (tokenData == null)
            return ApiResult<Verify2FaResponse>.Fail(401, "Invalid or expired token.");

        if (tokenData.Value.ClientId != client.ClientId)
            return ApiResult<Verify2FaResponse>.Fail(401, "Token not valid for this client.");

        var user = await userManager.FindByIdAsync(tokenData.Value.UserId.ToString());

        if (user is null)
            return ApiResult<Verify2FaResponse>.Fail(401, "User not found.");

        var isValidCode = await userManager.VerifyTwoFactorTokenAsync(user,
            userManager.Options.Tokens.AuthenticatorTokenProvider, command.Totp);

        if (!isValidCode)
            return ApiResult<Verify2FaResponse>.Fail(401, "Invalid authenticator code.");

        await signInManager.SignInAsync(user, isPersistent: false);

        return ApiResult<Verify2FaResponse>.Ok(new(user.Id));
    }
}