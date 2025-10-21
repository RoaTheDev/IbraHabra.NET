using IbraHabra.NET.Application.Dto;
using IbraHabra.NET.Domain.Constants;
using IbraHabra.NET.Domain.Contract;
using IbraHabra.NET.Domain.Entities;
using OpenIddict.Abstractions;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Client.Commands;

public record DeleteClientCommand(string ClientId);

public class DeleteClientHandler : IWolverineHandler
{
    public static async Task<ApiResult> Handle(DeleteClientCommand command, IRepo<OauthApplication, string> appRepo,
        IOpenIddictApplicationManager appManager)
    {
        var app = await appRepo.GetViaConditionAsync(a => a.ClientId == command.ClientId);
        if (app == null)
            return ApiResult.Fail(ApiErrors.OAuthApplication.NotFound());

        await appManager.DeleteAsync(app);
        return ApiResult.Ok();
    }
}