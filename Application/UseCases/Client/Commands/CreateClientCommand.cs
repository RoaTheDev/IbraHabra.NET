using System.Text.Json;
using IbraHabra.NET.Application.Dto.Response;
using IbraHabra.NET.Domain.Constants.ValueObject;
using IbraHabra.NET.Domain.Contract;
using IbraHabra.NET.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Client.Commands;

public record CreateClientCommand(
    Guid ProjectId,
    string? ClientSecret = null,
    string DisplayName = "",
    string? ApplicationType = null,
    string? ClientType = "public",
    string? ConsentType = "explicit",
    List<string>? RedirectUris = null,
    List<string>? PostLogoutRedirectUris = null,
    List<string>? Permissions = null,
    AuthPolicy? AuthPolicy = null);

public record CreateClientResponse(
    string Id,
    string ClientId,
    Guid ProjectId);

public class CreateClientHandler : IWolverineHandler
{
public static async Task<ApiResult<CreateClientResponse>> Handle(
    CreateClientCommand command,
    IRepo<Projects, Guid> projectRepo,
    IRepo<OauthApplication, string> appRepo,
    IUnitOfWork unitOfWork)
{
    var project = await projectRepo.GetViaIdAsync(command.ProjectId);
    if (project == null)
        return ApiResult<CreateClientResponse>.Fail(404, "Project not found.");

    var context = unitOfWork.DbContext;
    var strategy = context.Database.CreateExecutionStrategy();

    return await strategy.ExecuteAsync(async () =>
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            var authPolicy = command.AuthPolicy ?? new AuthPolicy
            {
                RequireDigit = false,
                RequireMfa = false,
                RequirePkce = false,
                RequireUppercase = false,
                MinPasswordLength = 8,
                RequireEmailVerification = false,
                RequireNonAlphanumeric = false
            };

            var policyJson = JsonSerializer.Serialize(authPolicy, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var propertiesDict = new Dictionary<string, JsonElement>
            {
                ["authPolicy"] = JsonSerializer.Deserialize<JsonElement>(policyJson)
            };

            var requirements = new List<string>();
            if (authPolicy.RequirePkce)
            {
                requirements.Add(OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange);
            }

            var app = new OauthApplication
            {
                Id = Guid.NewGuid().ToString(),
                ClientId = Guid.NewGuid().ToString(),
                ProjectId = project.Id,
                DisplayName = command.DisplayName,
                ClientType = command.ClientType,
                ClientSecret = command.ClientSecret,
                ConsentType = command.ConsentType,
                ApplicationType = command.ApplicationType,
                RedirectUris = JsonSerializer.Serialize(command.RedirectUris ?? new List<string>()),
                PostLogoutRedirectUris =
                    JsonSerializer.Serialize(command.PostLogoutRedirectUris ?? new List<string>()),
                Permissions = JsonSerializer.Serialize(command.Permissions ?? new List<string>()),
                Properties = JsonSerializer.Serialize(propertiesDict),
                Requirements = JsonSerializer.Serialize(requirements),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await appRepo.AddAsync(app);
            await unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();

            return ApiResult<CreateClientResponse>.Ok(
                new CreateClientResponse(app.Id, app.ClientId, project.Id));
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    });
}}