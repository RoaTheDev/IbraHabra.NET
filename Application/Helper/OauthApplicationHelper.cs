using IbraHabra.NET.Domain.Entity;
using IbraHabra.NET.Domain.ValueObject;

namespace IbraHabra.NET.Application.Helper;

using System.Text.Json;


public static class OauthApplicationHelper
{
    public static AuthPolicy GetAuthPolicy(this OauthApplication app)
    {
        if (string.IsNullOrEmpty(app.Properties))
            return new AuthPolicy();

        try
        {
            using var doc = JsonDocument.Parse(app.Properties);
            if (doc.RootElement.TryGetProperty("authPolicy", out var policyElement))
            {
                return policyElement.Deserialize<AuthPolicy>() ?? new AuthPolicy();
            }
        }
        catch (JsonException) { }

        return new AuthPolicy();
    }

    public static void SetAuthPolicy(this OauthApplication app, AuthPolicy policy)
    {
        var root = new { authPolicy = policy };
        app.Properties = JsonSerializer.Serialize(root, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }
}