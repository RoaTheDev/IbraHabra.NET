using OpenIddict.Abstractions;

namespace IbraHabra.NET.Domain.Constants;

public static class OauthConstantValidation
{
     public static readonly string[] ValidApplicationTypes =
    [
        OpenIddictConstants.ApplicationTypes.Native,
        OpenIddictConstants.ApplicationTypes.Web
    ];

    public static readonly string[] ValidClientTypes =
    [
        OpenIddictConstants.ClientTypes.Confidential,
        OpenIddictConstants.ClientTypes.Public
    ];

    public static readonly string[] ValidConsentTypes =
    [
        OpenIddictConstants.ConsentTypes.Explicit,
        OpenIddictConstants.ConsentTypes.External,
        OpenIddictConstants.ConsentTypes.Implicit,
        OpenIddictConstants.ConsentTypes.Systematic
    ];

    public static readonly string[] ValidPermissions =
    [
        // Endpoints
        OpenIddictConstants.Permissions.Endpoints.Authorization,
        OpenIddictConstants.Permissions.Endpoints.Token,
        OpenIddictConstants.Permissions.Endpoints.EndSession,
        OpenIddictConstants.Permissions.Endpoints.Introspection,
        OpenIddictConstants.Permissions.Endpoints.Revocation,
        OpenIddictConstants.Permissions.Endpoints.DeviceAuthorization,
        
        // Grant Types
        OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
        OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
        OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
        OpenIddictConstants.Permissions.GrantTypes.Implicit,
        OpenIddictConstants.Permissions.GrantTypes.Password,
        OpenIddictConstants.Permissions.GrantTypes.DeviceCode,
        
        // Response Types
        OpenIddictConstants.Permissions.ResponseTypes.Code,
        OpenIddictConstants.Permissions.ResponseTypes.Token,
        OpenIddictConstants.Permissions.ResponseTypes.IdToken,
        OpenIddictConstants.Permissions.ResponseTypes.CodeIdToken,
        OpenIddictConstants.Permissions.ResponseTypes.CodeToken,
        OpenIddictConstants.Permissions.ResponseTypes.IdTokenToken,
        OpenIddictConstants.Permissions.ResponseTypes.CodeIdTokenToken,
        OpenIddictConstants.Permissions.ResponseTypes.None,
        
        // Scopes
        OpenIddictConstants.Permissions.Scopes.Address,
        OpenIddictConstants.Permissions.Scopes.Email,
        OpenIddictConstants.Permissions.Scopes.Phone,
        OpenIddictConstants.Permissions.Scopes.Profile,
        OpenIddictConstants.Permissions.Scopes.Roles
    ];
    public static readonly string[] ValidPrefixes =
    [
        OpenIddictConstants.Permissions.Prefixes.Audience, // aud:
        OpenIddictConstants.Permissions.Prefixes.Endpoint, // ept:
        OpenIddictConstants.Permissions.Prefixes.GrantType, // gt:
        OpenIddictConstants.Permissions.Prefixes.ResponseType, // rst:
        OpenIddictConstants.Permissions.Prefixes.Resource, // rsrc:
        OpenIddictConstants.Permissions.Prefixes.Scope // scp:

    ];
    public static readonly string[] ValidPermissionPrefixes =
    [
        OpenIddictConstants.Permissions.Prefixes.Audience,
        OpenIddictConstants.Permissions.Prefixes.Endpoint,
        OpenIddictConstants.Permissions.Prefixes.GrantType,
        OpenIddictConstants.Permissions.Prefixes.ResponseType,
        OpenIddictConstants.Permissions.Prefixes.Resource,
        OpenIddictConstants.Permissions.Prefixes.Scope
    ];

}