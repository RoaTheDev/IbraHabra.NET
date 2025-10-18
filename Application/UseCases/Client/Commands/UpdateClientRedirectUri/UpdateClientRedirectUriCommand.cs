using System.Linq.Expressions;
using System.Text.Json;
using FluentValidation;
using IbraHabra.NET.Application.Dto;
using IbraHabra.NET.Application.Dto.Response;
using IbraHabra.NET.Application.Utils;
using IbraHabra.NET.Domain.Constants;
using IbraHabra.NET.Domain.Contract;
using IbraHabra.NET.Domain.Entities;
using Microsoft.EntityFrameworkCore.Query;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Client.Commands.UpdateClientRedirectUri;

public record UpdateClientRedirectUrisCommand(
    string ClientId,
    List<string> RedirectUris,
    List<string>? PostLogoutRedirectUris = null);

public class UpdateClientRedirectUrisHandler : IWolverineHandler
{
    public static async Task<ApiResult<string>> Handle(
        UpdateClientRedirectUrisCommand command,
        IRepo<OauthApplication, string> appRepo,
        IValidator<UpdateClientRedirectUrisCommand> validator)
    {
        var jsonedRedirectUris = JsonSerializer.Serialize(command.RedirectUris);
        var jsonedPostLogoutRedirectUris = JsonSerializer.Serialize(command.PostLogoutRedirectUris);
        Expression<Func<SetPropertyCalls<OauthApplication>, SetPropertyCalls<OauthApplication>>> update =
            s => s.SetProperty(p => p.UpdatedAt, DateTime.UtcNow)
                .SetProperty(x => x.RedirectUris, jsonedRedirectUris);

        if (command.PostLogoutRedirectUris != null)
        {
            update = CrudUtils.Append(update,
                s => s.SetProperty(p => p.PostLogoutRedirectUris, jsonedPostLogoutRedirectUris));
        }

        var rowsAffected = await appRepo.UpdateAsync(
            command.ClientId, update);

        if (rowsAffected == 0)
            return ApiResult<string>.Fail(ApiErrors.OAuthApplication.NotFound());

        return ApiResult<string>.Ok("Redirect URIs updated successfully.");
    }
}