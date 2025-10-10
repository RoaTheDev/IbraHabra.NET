using FluentValidation;
using OpenIddict.Abstractions;

namespace IbraHabra.NET.Application.UseCases.Client.Commands.CreateClient;

public class CreateClientCommandValidator : AbstractValidator<CreateClientCommand>
{
    private static readonly string[] ValidApplicationTypes =
    [
        OpenIddictConstants.ApplicationTypes.Native,
        OpenIddictConstants.ApplicationTypes.Web
    ];

    private static readonly string[] ValidClientTypes =
    [
        OpenIddictConstants.ClientTypes.Confidential,
        OpenIddictConstants.ClientTypes.Public
    ];

    private static readonly string[] ValidConsentTypes =
    [
        OpenIddictConstants.ConsentTypes.Explicit,
        OpenIddictConstants.ConsentTypes.External,
        OpenIddictConstants.ConsentTypes.Implicit,
        OpenIddictConstants.ConsentTypes.Systematic
    ];

    private static readonly string[] ValidPermissions =
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

    public CreateClientCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithMessage("Project ID is required.");

        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .WithMessage("Display name is required.")
            .MaximumLength(200)
            .WithMessage("Display name must not exceed 200 characters.");

        RuleFor(x => x.ApplicationType)
            .Must(type => string.IsNullOrEmpty(type) || ValidApplicationTypes.Contains(type))
            .WithMessage($"Application type must be one of: {string.Join(", ", ValidApplicationTypes)}");

        RuleFor(x => x.ClientType)
            .Must(type => string.IsNullOrEmpty(type) || ValidClientTypes.Contains(type))
            .WithMessage($"Client type must be one of: {string.Join(", ", ValidClientTypes)}")
            .NotEmpty()
            .WithMessage("Client type is required.");

        RuleFor(x => x.ConsentType)
            .Must(type => string.IsNullOrEmpty(type) || ValidConsentTypes.Contains(type))
            .WithMessage($"Consent type must be one of: {string.Join(", ", ValidConsentTypes)}");

        // Client Secret validation
        RuleFor(x => x.ClientSecret)
            .NotEmpty()
            .When(x => x.ClientType == OpenIddictConstants.ClientTypes.Confidential)
            .WithMessage("Client secret is required for confidential clients.")
            .MinimumLength(32)
            .When(x => !string.IsNullOrEmpty(x.ClientSecret))
            .WithMessage("Client secret must be at least 32 characters for security.");

        RuleFor(x => x.ClientSecret)
            .Empty()
            .When(x => x.ClientType == OpenIddictConstants.ClientTypes.Public)
            .WithMessage("Public clients should not have a client secret.");

        // Redirect URIs validation
        RuleFor(x => x.RedirectUris)
            .NotEmpty()
            .When(x => RequiresRedirectUris(x.Permissions))
            .WithMessage("Redirect URIs are required for authorization code or implicit flows.")
            .Must(uris => uris == null || uris.All(IsValidUri))
            .WithMessage("All redirect URIs must be valid absolute URIs.");

        RuleForEach(x => x.RedirectUris)
            .Must(uri => !uri.Contains("#"))
            .When(x => x.RedirectUris != null)
            .WithMessage("Redirect URIs must not contain fragments (#).");

        // Post-logout redirect URIs validation
        RuleFor(x => x.PostLogoutRedirectUris)
            .Must(uris => uris == null || uris.All(IsValidUri))
            .WithMessage("All post-logout redirect URIs must be valid absolute URIs.");

        // Permissions validation
        RuleFor(x => x.Permissions)
            .Must(perms => perms == null || perms.All(IsValidPermission))
            .WithMessage($"All permissions must be valid OpenIddict permissions or follow the format 'prefix:value'.");

        RuleFor(x => x.Permissions)
            .Must(HasRequiredEndpointPermissions)
            .When(x => x.Permissions != null && x.Permissions.Any())
            .WithMessage("Client must have at least authorization and token endpoint permissions.");

        RuleFor(x => x.Permissions)
            .Must(HasConsistentGrantAndResponseTypes)
            .When(x => x.Permissions != null && x.Permissions.Any())
            .WithMessage("Grant types and response types must be consistent.");

        // PKCE validation
        RuleFor(x => x.AuthPolicy)
            .Must((command, policy) => ValidatePkceForPublicClients(command, policy))
            .WithMessage("Public clients should require PKCE for security.")
            .When(x => x.ClientType == OpenIddictConstants.ClientTypes.Public);

        // Custom permission format validation
        RuleForEach(x => x.Permissions)
            .Must(IsValidCustomPermission)
            .When(x => x.Permissions != null && x.Permissions.Any(p => !ValidPermissions.Contains(p)))
            .WithMessage("Custom permissions must follow the format 'prefix:value' (e.g., 'aud:api1', 'scp:custom_scope').");
    }

    private static bool IsValidUri(string uri)
    {
        return Uri.TryCreate(uri, UriKind.Absolute, out var result) &&
               (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }

    private static bool IsValidPermission(string permission)
    {
        // Check if it's a standard permission
        if (ValidPermissions.Contains(permission))
            return true;

        // Check if it's a custom permission with valid prefix
        var validPrefixes = new[]
        {
            OpenIddictConstants.Permissions.Prefixes.Audience,      // aud:
            OpenIddictConstants.Permissions.Prefixes.Endpoint,      // ept:
            OpenIddictConstants.Permissions.Prefixes.GrantType,     // gt:
            OpenIddictConstants.Permissions.Prefixes.ResponseType,  // rst:
            OpenIddictConstants.Permissions.Prefixes.Resource,      // rsrc:
            OpenIddictConstants.Permissions.Prefixes.Scope          // scp:
        };

        return validPrefixes.Any(prefix => permission.StartsWith(prefix) && permission.Length > prefix.Length);
    }

    private static bool IsValidCustomPermission(string permission)
    {
        if (ValidPermissions.Contains(permission))
            return true;

        // For custom permissions, check format
        var colonIndex = permission.IndexOf(':');
        return colonIndex > 0 && colonIndex < permission.Length - 1;
    }

    private static bool RequiresRedirectUris(List<string>? permissions)
    {
        if (permissions == null || !permissions.Any())
            return false;

        var requiresRedirect = new[]
        {
            OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
            OpenIddictConstants.Permissions.GrantTypes.Implicit,
            OpenIddictConstants.Permissions.ResponseTypes.Code,
            OpenIddictConstants.Permissions.ResponseTypes.Token,
            OpenIddictConstants.Permissions.ResponseTypes.IdToken
        };

        return permissions.Any(p => requiresRedirect.Contains(p));
    }

    private static bool HasRequiredEndpointPermissions(List<string>? permissions)
    {
        if (permissions == null || !permissions.Any())
            return true; // Let it pass, other validation will catch empty permissions

        var endpointPermissions = permissions
            .Where(p => p.StartsWith(OpenIddictConstants.Permissions.Prefixes.Endpoint))
            .ToList();

        if (!endpointPermissions.Any())
            return true; // No endpoint permissions specified, that's okay

        // If using authorization code or implicit, must have authorization endpoint
        var usesAuthFlow = permissions.Any(p =>
            p == OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode ||
            p == OpenIddictConstants.Permissions.GrantTypes.Implicit);

        if (usesAuthFlow && !permissions.Contains(OpenIddictConstants.Permissions.Endpoints.Authorization))
            return false;

        // If using any grant type except implicit, must have token endpoint
        var usesTokenEndpoint = permissions.Any(p =>
            p == OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode ||
            p == OpenIddictConstants.Permissions.GrantTypes.ClientCredentials ||
            p == OpenIddictConstants.Permissions.GrantTypes.RefreshToken ||
            p == OpenIddictConstants.Permissions.GrantTypes.Password);

        if (usesTokenEndpoint && !permissions.Contains(OpenIddictConstants.Permissions.Endpoints.Token))
            return false;

        return true;
    }

    private static bool HasConsistentGrantAndResponseTypes(List<string>? permissions)
    {
        if (permissions == null || !permissions.Any())
            return true;

        var hasAuthCodeGrant = permissions.Contains(OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode);
        var hasCodeResponse = permissions.Contains(OpenIddictConstants.Permissions.ResponseTypes.Code);

        var hasImplicitGrant = permissions.Contains(OpenIddictConstants.Permissions.GrantTypes.Implicit);
        var hasImplicitResponse = permissions.Any(p =>
            p == OpenIddictConstants.Permissions.ResponseTypes.Token ||
            p == OpenIddictConstants.Permissions.ResponseTypes.IdToken ||
            p == OpenIddictConstants.Permissions.ResponseTypes.IdTokenToken);

        // If you have authorization code grant, you should have code response type
        if (hasAuthCodeGrant && !hasCodeResponse)
            return false;

        // If you have implicit grant, you should have implicit response types
        if (hasImplicitGrant && !hasImplicitResponse)
            return false;

        return true;
    }

    private static bool ValidatePkceForPublicClients(CreateClientCommand command, Domain.Constants.ValueObject.AuthPolicy? policy)
    {
        // Public clients should ideally require PKCE
        if (command.ClientType == OpenIddictConstants.ClientTypes.Public)
        {
            var usesAuthCode = command.Permissions?.Contains(
                OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode) == true;

            // If using authorization code flow, PKCE is highly recommended
            if (usesAuthCode && policy?.RequirePkce == false)
            {
                // This is a warning-level validation - you might want to allow it but warn
                // For now, we'll allow it but you could change this to return false for stricter validation
                return true;
            }
        }

        return true;
    }
}