using IbraHabra.NET.Application.Dto.Response;
using IbraHabra.NET.Domain.Contract;
using IbraHabra.NET.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Users.Commands;

public record LogoutUserCommand(Guid? UserId, string ClientId, bool RevokeAllTokens = false);

public class LogoutUserHandler : IWolverineHandler
{
    public static async Task<ApiResult> Handler(LogoutUserCommand command, IRepo<OauthApplication, string> repo,
        SignInManager<User> signInManager, UserManager<User> userManager, IOpenIddictTokenManager tokenManager)
    {
        var client = await repo.GetViaConditionAsync(c => c.ClientId == command.ClientId && c.IsActive);
        if (client == null)
            return ApiResult.Fail(400, "Invalid or inactive client.");

        await signInManager.SignOutAsync();
        if (command.RevokeAllTokens && command.UserId.HasValue)
        {
            var user = await userManager.FindByIdAsync(command.UserId.HasValue.ToString());
            if (user != null)
            {
                await foreach (var token in tokenManager.FindBySubjectAsync(user.Id.ToString()))
                {
                    await tokenManager.TryRevokeAsync(token);
                }
            }
        }

        return ApiResult.Ok();
    }
}