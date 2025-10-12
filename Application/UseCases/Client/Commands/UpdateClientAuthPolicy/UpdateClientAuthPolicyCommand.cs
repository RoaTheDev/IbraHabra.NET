using System.Text.Json;
using FluentValidation;
using IbraHabra.NET.Application.Dto.Response;
using IbraHabra.NET.Domain.Constants.ValueObject;
using IbraHabra.NET.Domain.Contract;
using IbraHabra.NET.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Client.Commands.UpdateClientAuthPolicy;

public record UpdateClientAuthPolicyCommand(
    string ClientId,
    AuthPolicy AuthPolicy);

public class UpdateClientAuthPolicyHandler : IWolverineHandler
{
    public static async Task<ApiResult<string>> Handle(
        UpdateClientAuthPolicyCommand command,
        IRepo<OauthApplication, string> appRepo,
        IValidator<UpdateClientAuthPolicyCommand> validator,
        IUnitOfWork unitOfWork)
    {
        var validationResult = await validator.ValidateAsync(command);
        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            return ApiResult<string>.Fail(400, errors);
        }

        var strategy = unitOfWork.DbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await unitOfWork.BeginTransactionAsync();

            try
            {
                var client = await appRepo.GetViaConditionAsync(c => c.ClientId == command.ClientId);
                if (client == null)
                    return ApiResult<string>.Fail(404, "Client not found.");

                var propertiesDict = string.IsNullOrEmpty(client.Properties)
                    ? new Dictionary<string, JsonElement>()
                    : JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(client.Properties)
                      ?? new Dictionary<string, JsonElement>();

                var policyJson = JsonSerializer.Serialize(command.AuthPolicy, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                propertiesDict["authPolicy"] = JsonSerializer.Deserialize<JsonElement>(policyJson);

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

                await unitOfWork.SaveChangesAsync();

                await unitOfWork.CommitTransactionAsync();

                return ApiResult<string>.Ok("Auth policy updated successfully.");
            }
            catch
            {
                await unitOfWork.RollbackTransactionAsync();
                throw;
            }
        });
    }
}