using System.Text.Json;
using FluentValidation;
using IbraHabra.NET.Application.Dto;
using IbraHabra.NET.Application.Dto.Response;
using IbraHabra.NET.Domain.Contract;
using IbraHabra.NET.Domain.Entities;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Client.Commands.UpdateClientPermission;

public abstract record UpdateClientPermissionsCommand(
    string ClientId,
    List<string> Permissions);

public class UpdateClientPermissionsHandler : IWolverineHandler
{
    public static async Task<ApiResult<string>> Handle(
        UpdateClientPermissionsCommand command,
        IRepo<OauthApplication, string> appRepo,
        IValidator<UpdateClientPermissionsCommand> validator)
    {
        var validationResult = await validator.ValidateAsync(command);
        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            return ApiResult<string>.Fail(400, errors);
        }

        var client = await appRepo.GetViaConditionAsync(c => c.ClientId == command.ClientId);
        if (client == null)
            return ApiResult<string>.Fail(404, "Client not found.");

        var normalizedPermissions = command.Permissions.Distinct().ToList();
        var jsonedPermission = JsonSerializer.Serialize(normalizedPermissions);

        var rowsAffected = await appRepo.UpdateAsync(
            command.ClientId, a => a
                .SetProperty(x => x.UpdatedAt, DateTime.UtcNow)
                .SetProperty(x => x.Permissions, jsonedPermission)
        );

        if (rowsAffected == 0)
            return ApiResult<string>.Fail(500, "Failed to update permissions.");

        return ApiResult<string>.Ok("Permissions updated successfully.");
    }
}