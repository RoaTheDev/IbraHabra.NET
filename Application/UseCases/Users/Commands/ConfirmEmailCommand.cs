using IbraHabra.NET.Application.Dto;
using IbraHabra.NET.Application.Dto.Response;
using IbraHabra.NET.Domain.Constants;
using IbraHabra.NET.Domain.Contract;
using IbraHabra.NET.Domain.Entities;
using ImTools;
using Microsoft.AspNetCore.Identity;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Users.Commands;

public record ConfirmEmailCommand(string Email, string Token, string ClientId);

public class ConfirmEmailHandler : IWolverineHandler
{
    public static async Task<ApiResult> Handle(ConfirmEmailCommand command, IRepo<OauthApplication, string> repo,
        UserManager<User> userManager)
    {
        var client = await repo.GetViaConditionAsync(c => c.ClientId == command.ClientId && c.IsActive);
        if (client == null)
            return ApiResult.Fail(ApiErrors.OAuthApplication.NotFound());

        var user = await userManager.FindByEmailAsync(command.Email);
        if (user is null) return ApiResult.Fail(ApiErrors.User.NotFound());

        var result = await userManager.ConfirmEmailAsync(user, command.Token);
        if (!result.Succeeded) return ApiResult.Fail(ApiErrors.User.InvalidEmailConfirmationToken());

        return ApiResult.Ok();
    }
}