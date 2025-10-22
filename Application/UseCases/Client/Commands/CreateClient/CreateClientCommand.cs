using System.Text.Json;
using FluentValidation;
using IbraHabra.NET.Application.Dto;
using IbraHabra.NET.Application.Dto.Response;
using IbraHabra.NET.Domain.Constants;
using IbraHabra.NET.Domain.Constants.Values;
using IbraHabra.NET.Domain.Contract;
using IbraHabra.NET.Domain.Contract.Services;
using IbraHabra.NET.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Client.Commands.CreateClient;

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
    string ClientId,
    Guid ProjectId);

public class CreateClientHandler : IWolverineHandler
{
    public static async Task<ApiResult<CreateClientResponse>> Handle(
        CreateClientCommand command,
        IRepo<Projects, Guid> projectRepo,
        IRepo<OauthApplication, string> appRepo,
        IUnitOfWork unitOfWork,
        IClientSecretHasher secretHasher)
    {
        var project = await projectRepo.GetViaIdAsync(command.ProjectId);
        if (project == null)
            return ApiResult<CreateClientResponse>.Fail(ApiErrors.Project.NotFound());

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
                    RequirePkce = command.ClientType == OpenIddictConstants.ClientTypes.Public,
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

                var permissions = NormalizePermissions(command.Permissions);

                string? hashedSecret = null;
                if (!string.IsNullOrEmpty(command.ClientSecret))
                {
                    hashedSecret = secretHasher.HashSecret(command.ClientSecret);
                }

                var app = new OauthApplication
                {
                    Id = Guid.NewGuid().ToString(),
                    ClientId = GenerateClientId(),
                    ProjectId = project.Id,
                    DisplayName = command.DisplayName,
                    ClientType = command.ClientType ?? OpenIddictConstants.ClientTypes.Public,
                    ClientSecret = hashedSecret,
                    ConsentType = command.ConsentType ?? OpenIddictConstants.ConsentTypes.Explicit,
                    ApplicationType = command.ApplicationType,
                    RedirectUris = JsonSerializer.Serialize(command.RedirectUris ?? new List<string>()),
                    PostLogoutRedirectUris = JsonSerializer.Serialize(
                        command.PostLogoutRedirectUris ?? new List<string>()),
                    Permissions = JsonSerializer.Serialize(permissions),
                    Properties = JsonSerializer.Serialize(propertiesDict),
                    Requirements = JsonSerializer.Serialize(requirements),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await appRepo.AddAsync(app);
                await unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();

                return ApiResult<CreateClientResponse>.Ok(
                    new CreateClientResponse( app.ClientId, project.Id));
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }

    private static string GenerateClientId()
    {
        return $"client_{Guid.NewGuid():N}";
    }

    private static List<string> NormalizePermissions(List<string>? permissions)
    {
        if (permissions == null || !permissions.Any())
        {
            return new List<string>
            {
                OpenIddictConstants.Permissions.Endpoints.Authorization,
                OpenIddictConstants.Permissions.Endpoints.Token,
                OpenIddictConstants.Permissions.Endpoints.EndSession,
                OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                OpenIddictConstants.Permissions.ResponseTypes.Code,
                OpenIddictConstants.Permissions.Scopes.Email,
                OpenIddictConstants.Permissions.Scopes.Profile,
                OpenIddictConstants.Permissions.Scopes.Roles
            };
        }

        return permissions.Distinct().ToList();
    }
}