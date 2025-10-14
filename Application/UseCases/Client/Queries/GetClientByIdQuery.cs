using IbraHabra.NET.Application.Dto;
using IbraHabra.NET.Application.Dto.Response;
using IbraHabra.NET.Domain.Contract;
using IbraHabra.NET.Domain.Entities;
using OpenIddict.Abstractions;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Client.Queries;

public record GetClientByIdQuery(string ClientId);

public record GetClientResponse(
    string Id,
    string ClientId,
    Guid ProjectId,
    string? DisplayName,
    string? ApplicationType,
    string? ClientType,
    string? ConsentType,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<string>? RedirectUris,
    List<string>? PostLogoutRedirectUris,
    List<string>? Permissions);

public class GetClientByIdHandler : IWolverineHandler
{
    public static async Task<ApiResult<GetClientResponse>> Handle(
        GetClientByIdQuery query,
        IRepo<OauthApplication, string> appRepo,
        IOpenIddictApplicationManager appManager)
    {
        var app = await appRepo.GetViaConditionAsync(a => a.ClientId == query.ClientId);

        if (app == null)
            return ApiResult<GetClientResponse>.Fail(404, "Client not found.");

        var redirectUris = (await appManager.GetRedirectUrisAsync(app))
            .Select(u => u.ToString()).ToList();

        var postLogoutUris = (await appManager.GetPostLogoutRedirectUrisAsync(app))
            .Select(u => u.ToString()).ToList();

        var permissions = (await appManager.GetPermissionsAsync(app)).ToList();

        return ApiResult<GetClientResponse>.Ok(new GetClientResponse(
            app.Id,
            app.ClientId!,
            app.ProjectId,
            app.DisplayName,
            app.ApplicationType,
            app.ClientType,
            app.ConsentType,
            app.IsActive,
            app.CreatedAt,
            app.UpdatedAt,
            redirectUris,
            postLogoutUris,
            permissions));
    }
}