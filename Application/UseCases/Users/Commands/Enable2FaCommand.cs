using System.Security.Claims;
using IbraHabra.NET.Application.Dto.Response;
using IbraHabra.NET.Domain.Contract;
using IbraHabra.NET.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Users.Commands;

public record Enable2FaCommand(ClaimsPrincipal Principal, string Code, string ClientId);

public record Enable2FaResponse(string[] RecoveryCodes);

public class Enable2FaHandler : IWolverineHandler
{
    public static async Task<ApiResult<Enable2FaResponse>> Handler(Enable2FaCommand command,
        UserManager<User> userManager,
        IRepo<OauthApplication, string> repo)
    {
        if (!await repo.ExistsAsync(c => c.ClientId == command.ClientId && c.IsActive))
            return ApiResult<Enable2FaResponse>.Fail(404, "The client does not exist");

        var user = await userManager.GetUserAsync(command.Principal);
        if (user == null)
            return ApiResult<Enable2FaResponse>.Fail(401, "Not authenticated.");

        if (await userManager.GetTwoFactorEnabledAsync(user))
            return ApiResult<Enable2FaResponse>.Fail(400, "2FA is already enabled.");

        var isValidCode = await userManager.VerifyTwoFactorTokenAsync(
            user,
            userManager.Options.Tokens.AuthenticatorTokenProvider,
            command.Code);

        if (!isValidCode)
            return ApiResult<Enable2FaResponse>.Fail(400, "Invalid authenticator code. Please try again.");

        var result = await userManager.SetTwoFactorEnabledAsync(user, true);
        if (!result.Succeeded)
            return ApiResult<Enable2FaResponse>.Fail(500, "Failed to enable 2FA. Please try again.");

        var recoveryCodes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

        return ApiResult<Enable2FaResponse>.Ok(new(recoveryCodes!.ToArray()));
    }
}