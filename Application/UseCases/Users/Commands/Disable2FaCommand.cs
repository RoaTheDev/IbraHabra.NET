using System.Security.Claims;
using IbraHabra.NET.Application.Dto.Response;
using IbraHabra.NET.Domain.Contract;
using IbraHabra.NET.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Users.Commands;

public record Disable2FaCommand(ClaimsPrincipal Principal, string ClientId);

public class Disable2FaHandler : IWolverineHandler
{
    public static async Task<ApiResult> Handle(
        Disable2FaCommand command,
        UserManager<User> userManager,
        IRepo<OauthApplication, string> repo)
    {
        if (!await repo.ExistsAsync(c => c.ClientId == command.ClientId && c.IsActive)) ;

        var user = await userManager.GetUserAsync(command.Principal);
        if (user == null)
            return ApiResult.Fail(401, "Not authenticated.");

        if (!await userManager.GetTwoFactorEnabledAsync(user))
            return ApiResult.Fail(400, "2FA is not enabled.");

        var result = await userManager.SetTwoFactorEnabledAsync(user, false);
        if (!result.Succeeded)
            return ApiResult.Fail(500, "Failed to disable 2FA.");

        await userManager.ResetAuthenticatorKeyAsync(user);

        return ApiResult.Ok();
    }
}