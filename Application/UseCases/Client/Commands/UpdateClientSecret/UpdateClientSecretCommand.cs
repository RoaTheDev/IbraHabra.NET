using FluentValidation;
using IbraHabra.NET.Application.Dto;
using IbraHabra.NET.Domain.Constants;
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
        var clientType = await appRepo.GetViaConditionAsync(c => c.ClientId == command.ClientId,
            oa => oa.ClientType
        );

        if (clientType != OpenIddictConstants.ClientTypes.Confidential)
            return ApiResult<string>.Fail(ApiErrors.OAuthApplication.SecretKeyRuleViolation());

        var hashedSecret = secretHasher.HashSecret(command.NewClientSecret);

        var rowsAffected = await appRepo.UpdateAsync(
            command.ClientId, a => a
                .SetProperty(x => x.UpdatedAt, DateTime.UtcNow)
                .SetProperty(x => x.ClientSecret, hashedSecret)
        );

        if (rowsAffected == 0)
            return ApiResult<string>.Fail(ApiErrors.OAuthApplication.NotFound());

        return ApiResult<string>.Ok("Client secret updated successfully.");
    }
}