using System.Text.Json;
using IbraHabra.NET.Application.Dto.Response;
using IbraHabra.NET.Domain.Constants.ValueObject;
using IbraHabra.NET.Domain.Contract;
using IbraHabra.NET.Domain.Entities;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Client.Commands;

public record UpdateClientAuthPolicyCommand(
    string ClientId,
    AuthPolicy AuthPolicy);

public class UpdateClientAuthPolicyHandler : IWolverineHandler
{
    public static async Task<ApiResult<string>> Handle(
        UpdateClientAuthPolicyCommand command,
        IRepo<OauthApplication, string> appRepo)
    {
        var authPolicy = command.AuthPolicy;

        if (authPolicy.MinPasswordLength < 6)
        {
            return ApiResult<string>.Fail(
                400,
                "MinPasswordLength must be at least 6."
            );
        }


        var policyJson = JsonSerializer.Serialize(authPolicy, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var rowsAffected = await appRepo.UpdateAsync(
            command.ClientId, a => a
                .SetProperty(x => x.UpdatedAt, DateTime.UtcNow)
                .SetProperty(x => x.Properties, policyJson)
        );

        if (rowsAffected == 0)
            return ApiResult<string>.Fail(404, "Client not found.");

        return ApiResult<string>.Ok("Auth policy updated successfully.");
    }
}