using IbraHabra.NET.Application.Dto;
using IbraHabra.NET.Domain.Constants;
using IbraHabra.NET.Domain.Contract;
using IbraHabra.NET.Domain.Contract.Services;
using IbraHabra.NET.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Users.Commands;

public record Enable2FaComplianceCommand(string SessionKey, string Code, string ClientId);

public record Enable2FaComplianceResponse(string[] RecoveryCodes, Guid UserId);

public class Enable2FComplianceHandler : IWolverineHandler
{
    public static async Task<ApiResult<Enable2FaComplianceResponse>> Handle(
        Enable2FaComplianceCommand command,
        UserManager<User> userManager,
        SignInManager<User> signInManager, ICacheService cache)
    {
        var cacheKey = $"compliance_2fa_{command.SessionKey}_{command.ClientId}";
        var cache2Fa = await cache.GetAsync<Client2FaCache>(cacheKey);
        if (cache2Fa is null)
            return ApiResult<Enable2FaComplianceResponse>.Fail(ApiErrors.Authentication.InvalidToken());

        if (cache2Fa.ClientId != command.ClientId)
            return ApiResult<Enable2FaComplianceResponse>.Fail(ApiErrors.OAuthApplication.InvalidClient());

        var user = await userManager.FindByIdAsync(cache2Fa.Id.ToString());
        if (user == null)
            return ApiResult<Enable2FaComplianceResponse>.Fail(ApiErrors.User.NotFound());

        var isValidCode = await userManager.VerifyTwoFactorTokenAsync(
            user,
            userManager.Options.Tokens.AuthenticatorTokenProvider,
            command.Code);

        if (!isValidCode)
            return ApiResult<Enable2FaComplianceResponse>.Fail(ApiErrors.Authentication.InvalidTwoFactorCode());

        var result = await userManager.SetTwoFactorEnabledAsync(user, true);
        if (!result.Succeeded)
            return ApiResult<Enable2FaComplianceResponse>.Fail(
                ApiErrors.User.FailToEnable2Fa(string.Join(", ", result.Errors)));

        var recoveryCodes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

        await signInManager.SignInAsync(user, isPersistent: false);

        return ApiResult<Enable2FaComplianceResponse>.Ok(new(
            RecoveryCodes: recoveryCodes!.ToArray(),
            UserId: user.Id));
    }
}