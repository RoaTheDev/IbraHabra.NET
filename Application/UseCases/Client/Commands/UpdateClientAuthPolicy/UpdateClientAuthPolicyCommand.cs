using System.Text.Json;
using FluentValidation;
using IbraHabra.NET.Application.Dto.Response;
using IbraHabra.NET.Domain.Constants.ValueObject;
using IbraHabra.NET.Domain.Contract;
using IbraHabra.NET.Domain.Entities;
using OpenIddict.Abstractions;
using Wolverine;
using Wolverine.Attributes;

namespace IbraHabra.NET.Application.UseCases.Client.Commands.UpdateClientAuthPolicy;

public record UpdateClientAuthPolicyCommand(
    string ClientId,
    AuthPolicy AuthPolicy);

[Transactional]
public class UpdateClientAuthPolicyHandler : IWolverineHandler
{
    public static async Task<ApiResult<string>> Handle(
        UpdateClientAuthPolicyCommand command,
        IRepo<OauthApplication, string> appRepo,
        IValidator<UpdateClientAuthPolicyCommand> validator)
    {
        var validationResult = await validator.ValidateAsync(command);
        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            return ApiResult<string>.Fail(400, errors);
        }

        // Get existing client to preserve other properties
        var client = await appRepo.GetViaIdAsync(command.ClientId);
        if (client == null)
            return ApiResult<string>.Fail(404, "Client not found.");

        // Deserialize existing properties or create new
        var propertiesDict = string.IsNullOrEmpty(client.Properties)
            ? new Dictionary<string, JsonElement>()
            : JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(client.Properties)
              ?? new Dictionary<string, JsonElement>();

        // Update auth policy
        var policyJson = JsonSerializer.Serialize(command.AuthPolicy, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        propertiesDict["authPolicy"] = JsonSerializer.Deserialize<JsonElement>(policyJson);

        // Update requirements if PKCE changed
        var requirements = string.IsNullOrEmpty(client.Requirements)
            ? new List<string>()
            : JsonSerializer.Deserialize<List<string>>(client.Requirements) ?? new List<string>();

        if (command.AuthPolicy.RequirePkce &&
            !requirements.Contains(OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange))
        {
            requirements.Add(OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange);
        }
        else if (!command.AuthPolicy.RequirePkce)
        {
            requirements.Remove(OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange);
        }

        client.Properties = JsonSerializer.Serialize(propertiesDict);
        client.Requirements = JsonSerializer.Serialize(requirements);
        appRepo.Update(client);

        return ApiResult<string>.Ok("Auth policy updated successfully.");
    }
}