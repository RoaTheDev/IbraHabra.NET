using System.Linq.Expressions;
using FluentValidation;
using IbraHabra.NET.Application.Dto.Response;
using IbraHabra.NET.Application.Utils;
using IbraHabra.NET.Domain.Contract;
using IbraHabra.NET.Domain.Entities;
using Microsoft.EntityFrameworkCore.Query;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Client.Commands.UpdateClientMetadata;

public record UpdateClientMetadataCommand(
    string ClientId,
    string? DisplayName = null,
    string? ApplicationType = null,
    string? ConsentType = null);

public class UpdateClientMetadataHandler : IWolverineHandler
{
    public static async Task<ApiResult<string>> Handle(
        UpdateClientMetadataCommand command,
        IRepo<OauthApplication, string> appRepo,
        IValidator<UpdateClientMetadataCommand> validator)
    {
        var validationResult = await validator.ValidateAsync(command);
        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            return ApiResult<string>.Fail(400, errors);
        }

        var client = await appRepo.GetViaIdAsync(command.ClientId);
        if (client == null)
            return ApiResult<string>.Fail(404, "Client not found.");

        Expression<Func<SetPropertyCalls<OauthApplication>, SetPropertyCalls<OauthApplication>>> update =
            s => s.SetProperty(p => p.UpdatedAt, DateTime.UtcNow);


        if (!string.IsNullOrEmpty(command.DisplayName))
            update = CrudUtils.Append(update, c => c.SetProperty(p => p.DisplayName, command.DisplayName));

        if (!string.IsNullOrEmpty(command.ApplicationType))
            update = CrudUtils.Append(update, c => c.SetProperty(p => p.ApplicationType, command.ApplicationType));


        if (!string.IsNullOrEmpty(command.ConsentType))
            update = CrudUtils.Append(update, c => c.SetProperty(p => p.ConsentType, command.ConsentType));

        var rowsAffected = await appRepo.UpdateAsync(
            command.ClientId, update);

        if (rowsAffected == 0)
            return ApiResult<string>.Fail(500, "Failed to update client metadata.");

        return ApiResult<string>.Ok("Client metadata updated successfully.");
    }
}