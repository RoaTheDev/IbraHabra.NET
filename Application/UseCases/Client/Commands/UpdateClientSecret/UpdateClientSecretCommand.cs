using FluentValidation;
using IbraHabra.NET.Application.Dto;
using IbraHabra.NET.Application.Dto.Response;
using IbraHabra.NET.Domain.Contract;
using IbraHabra.NET.Domain.Contract.Services;
using IbraHabra.NET.Domain.Entities;
using OpenIddict.Abstractions;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Client.Commands.UpdateClientSecret;

public record UpdateClientSecretCommand(
    string ClientId,
    string NewClientSecret);

public class UpdateClientSecretHandler : IWolverineHandler
{
    public static async Task<ApiResult<string>> Handle(
        UpdateClientSecretCommand command,
        IRepo<OauthApplication, string> appRepo,
        IValidator<UpdateClientSecretCommand> validator,
        IClientSecretHasher secretHasher)
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

        if (client.ClientType != OpenIddictConstants.ClientTypes.Confidential)
            return ApiResult<string>.Fail(400, "Only confidential clients can have client secrets.");

        var hashedSecret = secretHasher.HashSecret(command.NewClientSecret);

        var rowsAffected = await appRepo.UpdateAsync(
            command.ClientId, a => a
                .SetProperty(x => x.UpdatedAt, DateTime.UtcNow)
                .SetProperty(x => x.ClientSecret, hashedSecret)
        );

        if (rowsAffected == 0)
            return ApiResult<string>.Fail(500, "Failed to update client secret.");

        return ApiResult<string>.Ok("Client secret updated successfully.");
    }
}