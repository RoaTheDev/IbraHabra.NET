using IbraHabra.NET.Application.Dto;
using IbraHabra.NET.Application.Utils;
using IbraHabra.NET.Domain.Constants;
using IbraHabra.NET.Domain.Contract;
using IbraHabra.NET.Domain.Entities;
using OpenIddict.Abstractions;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Client.Commands;

public record PatchClientCommand(
    string ClientId,
    string? DisplayName,
    string? ApplicationType,
    string? ClientType,
    string? ConsentType,
    List<string>? RedirectUris,
    List<string>? PostLogoutRedirectUris,
    List<string>? Permissions);

public class PatchClientHandler : IWolverineHandler
{
    public async static Task<ApiResult> Handle(PatchClientCommand command, IRepo<OauthApplication, string> appRepo,
        IOpenIddictApplicationManager appManager)
    {
        var app = await appRepo.GetViaConditionAsync(a => a.ClientId == command.ClientId);
        if (app == null)
            return ApiResult.Fail(ApiErrors.OAuthApplication.NotFound());

        var descriptor = new OpenIddictApplicationDescriptor();
        await appManager.PopulateAsync(descriptor, app);

        if (command.DisplayName is not null)
            descriptor.DisplayName = command.DisplayName;
        if (command.ApplicationType is not null)
            descriptor.ApplicationType = command.ApplicationType;
        if (command.ClientType is not null)
            descriptor.ClientType = command.ClientType;
        if (command.ConsentType is not null)
            descriptor.ConsentType = command.ConsentType;

        if (command.RedirectUris is not null)
        {
            descriptor.RedirectUris.Clear();
            foreach (var uriStr in command.RedirectUris)
            {
                if (!AuthUtils.TryCreateUri(uriStr, out var uri))
                    return  ApiResult.Fail(ApiErrors.OAuthApplication.InvalidRedirectUri());
                descriptor.RedirectUris.Add(uri!);
            }
        }

        if (command.PostLogoutRedirectUris is not null)
        {
            descriptor.PostLogoutRedirectUris.Clear();
            foreach (var uriStr in command.PostLogoutRedirectUris)
            {
                if (!AuthUtils.TryCreateUri(uriStr, out var uri))
                    return ApiResult.Fail(ApiErrors.OAuthApplication.InvalidRedirectUri());
                descriptor.PostLogoutRedirectUris.Add(uri!);
            }
        }

        if (command.Permissions is not null)
        {
            descriptor.Permissions.Clear();
            foreach (var p in command.Permissions)
            {
                if (!string.IsNullOrWhiteSpace(p))
                    descriptor.Permissions.Add(p);
            }
        }

        app.UpdatedAt = DateTime.UtcNow;
        await appManager.UpdateAsync(app, descriptor);
        

        return ApiResult.Ok();
    }
}