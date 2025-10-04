using System.Text.Json;
using IbraHabra.NET.Domain.Constants.ValueObject;
using IbraHabra.NET.Domain.Contract;
using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;

namespace IbraHabra.NET.Domain.Entities;

public class OauthApplication : OpenIddictEntityFrameworkCoreApplication, IEntity<string>
{
    public override string Id { get; set; } = null!;
    public Guid ProjectId { get; set; }
    public virtual Projects Projects { get; set; } = null!;

    public bool IsActive { get; set; } = true;  

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }


    public async Task<AuthPolicy> GetAuthPolicy(IOpenIddictApplicationManager manager,
        CancellationToken cancellationToken = default)
    {
        var properties = await manager.GetPropertiesAsync(this, cancellationToken);
        if (properties.TryGetValue("authPolicy", out var policyElement))
        {
            try
            {
                var jsonString = policyElement.ToString();
                return JsonSerializer.Deserialize<AuthPolicy>(jsonString, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }) ?? new AuthPolicy();
            }
            catch (JsonException)
            {
                // Log the exception if needed
            }
        }

        return new AuthPolicy();
    }

    public void InitializeAuthPolicy(AuthPolicy policy)
    {
        var policyJson = JsonSerializer.Serialize(policy, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var propertiesDict = new Dictionary<string, JsonElement>
        {
            ["authPolicy"] = JsonSerializer.Deserialize<JsonElement>(policyJson)
        };
        Properties = JsonSerializer.Serialize(propertiesDict);

        var requirements = new List<string>();
        if (policy.RequirePkce)
        {
            requirements.Add(OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange);
        }
        Requirements = JsonSerializer.Serialize(requirements);
    }}