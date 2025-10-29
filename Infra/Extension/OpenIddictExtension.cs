using System.Text.Json;
using OpenIddict.EntityFrameworkCore.Models;

namespace IbraHabra.NET.Infra.Extension;

public static class OpenIddictApplicationExtensions
{
    // RedirectUris
    public static List<string> GetRedirectUris(this OpenIddictEntityFrameworkCoreApplication app)
    {
        if (string.IsNullOrEmpty(app.RedirectUris))
            return new List<string>();

        try
        {
            return JsonSerializer.Deserialize<List<string>>(app.RedirectUris) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    public static void SetRedirectUris(this OpenIddictEntityFrameworkCoreApplication app, List<string> uris)
    {
        app.RedirectUris = uris.Count > 0 ? JsonSerializer.Serialize(uris) : null;
    }

    // PostLogoutRedirectUris
    public static List<string> GetPostLogoutRedirectUris(this OpenIddictEntityFrameworkCoreApplication app)
    {
        if (string.IsNullOrEmpty(app.PostLogoutRedirectUris))
            return new List<string>();

        try
        {
            return JsonSerializer.Deserialize<List<string>>(app.PostLogoutRedirectUris) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    public static void SetPostLogoutRedirectUris(this OpenIddictEntityFrameworkCoreApplication app, List<string> uris)
    {
        app.PostLogoutRedirectUris = uris.Count > 0 ? JsonSerializer.Serialize(uris) : null;
    }

    // Permissions
    public static List<string> GetPermissions(this OpenIddictEntityFrameworkCoreApplication app)
    {
        if (string.IsNullOrEmpty(app.Permissions))
            return new List<string>();

        try
        {
            return JsonSerializer.Deserialize<List<string>>(app.Permissions) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    public static void SetPermissions(this OpenIddictEntityFrameworkCoreApplication app, List<string> permissions)
    {
        app.Permissions = permissions.Count > 0 ? JsonSerializer.Serialize(permissions) : null;
    }

    // Requirements
    public static List<string> GetRequirements(this OpenIddictEntityFrameworkCoreApplication app)
    {
        if (string.IsNullOrEmpty(app.Requirements))
            return new List<string>();

        try
        {
            return JsonSerializer.Deserialize<List<string>>(app.Requirements) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    public static void SetRequirements(this OpenIddictEntityFrameworkCoreApplication app, List<string> requirements)
    {
        app.Requirements = requirements.Count > 0 ? JsonSerializer.Serialize(requirements) : null;
    }

    // Properties (Dictionary)
    public static Dictionary<string, object> GetProperties(this OpenIddictEntityFrameworkCoreApplication app)
    {
        if (string.IsNullOrEmpty(app.Properties))
            return new Dictionary<string, object>();

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(app.Properties)
                   ?? new Dictionary<string, object>();
        }
        catch
        {
            return new Dictionary<string, object>();
        }
    }

    public static void SetProperties(this OpenIddictEntityFrameworkCoreApplication app,
        Dictionary<string, object> properties)
    {
        app.Properties = properties.Count > 0 ? JsonSerializer.Serialize(properties) : null;
    }

    // Settings (Dictionary)
    public static Dictionary<string, object> GetSettings(this OpenIddictEntityFrameworkCoreApplication app)
    {
        if (string.IsNullOrEmpty(app.Settings))
            return new Dictionary<string, object>();

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(app.Settings)
                   ?? new Dictionary<string, object>();
        }
        catch
        {
            return new Dictionary<string, object>();
        }
    }

    public static void SetSettings(this OpenIddictEntityFrameworkCoreApplication app,
        Dictionary<string, object> settings)
    {
        app.Settings = settings.Count > 0 ? JsonSerializer.Serialize(settings) : null;
    }

    // DisplayNames (Dictionary)
    public static Dictionary<string, string> GetDisplayNames(this OpenIddictEntityFrameworkCoreApplication app)
    {
        if (string.IsNullOrEmpty(app.DisplayNames))
            return new Dictionary<string, string>();

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(app.DisplayNames)
                   ?? new Dictionary<string, string>();
        }
        catch
        {
            return new Dictionary<string, string>();
        }
    }

    public static void SetDisplayNames(this OpenIddictEntityFrameworkCoreApplication app,
        Dictionary<string, string> displayNames)
    {
        app.DisplayNames = displayNames.Count > 0 ? JsonSerializer.Serialize(displayNames) : null;
    }

    // Extract origins from redirect URIs (for CORS)
    public static List<string> GetOrigins(this OpenIddictEntityFrameworkCoreApplication app)
    {
        var redirectUris = app.GetRedirectUris();
        var origins = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var uri in redirectUris)
        {
            if (Uri.TryCreate(uri, UriKind.Absolute, out var parsedUri))
            {
                var origin = $"{parsedUri.Scheme}://{parsedUri.Authority}";
                origins.Add(origin);
            }
        }

        return origins.ToList();
    }
}

public static class OpenIddictAuthorizationExtensions
{
    // Scopes
    public static List<string> GetScopes(this OpenIddictEntityFrameworkCoreAuthorization auth)
    {
        if (string.IsNullOrEmpty(auth.Scopes))
            return new List<string>();

        try
        {
            return JsonSerializer.Deserialize<List<string>>(auth.Scopes) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    public static void SetScopes(this OpenIddictEntityFrameworkCoreAuthorization auth, List<string> scopes)
    {
        auth.Scopes = scopes.Count > 0 ? JsonSerializer.Serialize(scopes) : null;
    }

    // Properties
    public static Dictionary<string, object> GetProperties(this OpenIddictEntityFrameworkCoreAuthorization auth)
    {
        if (string.IsNullOrEmpty(auth.Properties))
            return new Dictionary<string, object>();

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(auth.Properties)
                   ?? new Dictionary<string, object>();
        }
        catch
        {
            return new Dictionary<string, object>();
        }
    }

    public static void SetProperties(this OpenIddictEntityFrameworkCoreAuthorization auth,
        Dictionary<string, object> properties)
    {
        auth.Properties = properties.Count > 0 ? JsonSerializer.Serialize(properties) : null;
    }
}

public static class OpenIddictScopeExtensions
{
    // Resources
    public static List<string> GetResources(this OpenIddictEntityFrameworkCoreScope scope)
    {
        if (string.IsNullOrEmpty(scope.Resources))
            return new List<string>();

        try
        {
            return JsonSerializer.Deserialize<List<string>>(scope.Resources) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    public static void SetResources(this OpenIddictEntityFrameworkCoreScope scope, List<string> resources)
    {
        scope.Resources = resources.Count > 0 ? JsonSerializer.Serialize(resources) : null;
    }

    // Properties
    public static Dictionary<string, object> GetProperties(this OpenIddictEntityFrameworkCoreScope scope)
    {
        if (string.IsNullOrEmpty(scope.Properties))
            return new Dictionary<string, object>();

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(scope.Properties)
                   ?? new Dictionary<string, object>();
        }
        catch
        {
            return new Dictionary<string, object>();
        }
    }

    public static void SetProperties(this OpenIddictEntityFrameworkCoreScope scope,
        Dictionary<string, object> properties)
    {
        scope.Properties = properties.Count > 0 ? JsonSerializer.Serialize(properties) : null;
    }

    // DisplayNames
    public static Dictionary<string, string> GetDisplayNames(this OpenIddictEntityFrameworkCoreScope scope)
    {
        if (string.IsNullOrEmpty(scope.DisplayNames))
            return new Dictionary<string, string>();

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(scope.DisplayNames)
                   ?? new Dictionary<string, string>();
        }
        catch
        {
            return new Dictionary<string, string>();
        }
    }

    public static void SetDisplayNames(this OpenIddictEntityFrameworkCoreScope scope,
        Dictionary<string, string> displayNames)
    {
        scope.DisplayNames = displayNames.Count > 0 ? JsonSerializer.Serialize(displayNames) : null;
    }

    // Descriptions
    public static Dictionary<string, string> GetDescriptions(this OpenIddictEntityFrameworkCoreScope scope)
    {
        if (string.IsNullOrEmpty(scope.Descriptions))
            return new Dictionary<string, string>();

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(scope.Descriptions)
                   ?? new Dictionary<string, string>();
        }
        catch
        {
            return new Dictionary<string, string>();
        }
    }

    public static void SetDescriptions(this OpenIddictEntityFrameworkCoreScope scope,
        Dictionary<string, string> descriptions)
    {
        scope.Descriptions = descriptions.Count > 0 ? JsonSerializer.Serialize(descriptions) : null;
    }
}

public static class OpenIddictTokenExtensions
{
    // Properties
    public static Dictionary<string, object> GetProperties(this OpenIddictEntityFrameworkCoreToken token)
    {
        if (string.IsNullOrEmpty(token.Properties))
            return new Dictionary<string, object>();

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(token.Properties)
                   ?? new Dictionary<string, object>();
        }
        catch
        {
            return new Dictionary<string, object>();
        }
    }

    public static void SetProperties(this OpenIddictEntityFrameworkCoreToken token,
        Dictionary<string, object> properties)
    {
        token.Properties = properties.Count > 0 ? JsonSerializer.Serialize(properties) : null;
    }
}