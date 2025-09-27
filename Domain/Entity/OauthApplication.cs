using System.Text.Json;
using IbraHabra.NET.Domain.Interface;
using IbraHabra.NET.Domain.ValueObject;
using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;

namespace IbraHabra.NET.Domain.Entity;

public class OauthApplication : OpenIddictEntityFrameworkCoreApplication, IEntity<string>
{
    public override string Id => base.Id!;

    public Guid ProjectId { get; set; }
    public virtual Projects Projects { get; set; } = null!;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public AuthPolicy GetAuthPolicy()
    {
        if (string.IsNullOrEmpty(Properties))
            return new AuthPolicy();

        try
        {
            using var document = JsonDocument.Parse(Properties);
            if (document.RootElement.TryGetProperty("authPolicy", out var policyElement))
            {
                return JsonSerializer.Deserialize<AuthPolicy>(policyElement.GetRawText(), new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }) ?? new AuthPolicy();
            }
        }
        catch (JsonException)
        {
            // Log the exception if needed
        }

        return new AuthPolicy();
    }
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

    public async Task SetAuthPolicy(IOpenIddictApplicationManager manager, AuthPolicy policy,
        CancellationToken cancellationToken = default)
    {
        var descriptor = new OpenIddictApplicationDescriptor();
        await manager.PopulateAsync(descriptor, this, cancellationToken);

        var policyJson = JsonSerializer.Serialize(policy, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        descriptor.Properties["authPolicy"] = JsonSerializer.Deserialize<JsonElement>(policyJson);

        var pkceRequirement = OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange;

        if (policy.RequirePkce)
        {
            descriptor.Requirements.Add(pkceRequirement);
        }
        else
        {
            descriptor.Requirements.Remove(pkceRequirement);
        }

        await manager.UpdateAsync(this, descriptor, cancellationToken);
    }
}