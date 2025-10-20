using IbraHabra.NET.Application.Dto;
using IbraHabra.NET.Domain.Constants;
using IbraHabra.NET.Domain.Contract;
using IbraHabra.NET.Domain.Contract.Services;
using IbraHabra.NET.Domain.Entities;
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
            return ApiResult<Verify2FaResponse>.Fail(ApiErrors.OAuthApplication.NotFound());

        var tokenData = await tokenService.ValidateAndRemoveTokenAsync(command.TwoFactorToken);
        if (tokenData == null)
            return ApiResult<Verify2FaResponse>.Fail(ApiErrors.User.InvalidTwoFactorCode());

        if (tokenData.Value.ClientId != client.ClientId)
            return ApiResult<Verify2FaResponse>.Fail(ApiErrors.OAuthApplication.InvalidClient());

        var user = await userManager.FindByIdAsync(tokenData.Value.UserId.ToString());

        if (user is null)
            return ApiResult<Verify2FaResponse>.Fail(ApiErrors.User.NotFound());

        var isValidCode = await userManager.VerifyTwoFactorTokenAsync(user,
            userManager.Options.Tokens.AuthenticatorTokenProvider, command.Totp);

        if (!isValidCode)
            return ApiResult<Verify2FaResponse>.Fail(ApiErrors.User.InvalidTwoFactorCode());

        await signInManager.SignInAsync(user, isPersistent: false);

        return ApiResult<Verify2FaResponse>.Ok(new(user.Id));
    }
}