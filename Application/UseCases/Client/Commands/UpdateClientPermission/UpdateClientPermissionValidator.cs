using FluentValidation;
using OpenIddict.Abstractions;

namespace IbraHabra.NET.Application.UseCases.Client.Commands.UpdateClientPermission;

public class UpdateClientPermissionsValidator : AbstractValidator<UpdateClientPermissionsCommand>
{
    private static readonly string[] ValidPermissionPrefixes =
    [
        OpenIddictConstants.Permissions.Prefixes.Audience,
        OpenIddictConstants.Permissions.Prefixes.Endpoint,
        OpenIddictConstants.Permissions.Prefixes.GrantType,
        OpenIddictConstants.Permissions.Prefixes.ResponseType,
        OpenIddictConstants.Permissions.Prefixes.Resource,
        OpenIddictConstants.Permissions.Prefixes.Scope
    ];

    public UpdateClientPermissionsValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty()
            .WithMessage("Client ID is required.");

        RuleFor(x => x.Permissions)
            .NotEmpty()
            .WithMessage("At least one permission is required.");

        RuleForEach(x => x.Permissions)
            .Must(BeValidPermission)
            .WithMessage("All permissions must be valid OpenIddict permissions.");

        RuleFor(x => x.Permissions)
            .Must(HaveConsistentConfiguration)
            .WithMessage("Permissions must have consistent grant types and response types.");
    }

    private bool BeValidPermission(string permission)
    {
        return ValidPermissionPrefixes.Any(prefix => permission.StartsWith(prefix));
    }

    private bool HaveConsistentConfiguration(List<string> permissions)
    {
        var hasAuthCodeGrant = permissions.Contains(OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode);
        var hasCodeResponse = permissions.Contains(OpenIddictConstants.Permissions.ResponseTypes.Code);

        if (hasAuthCodeGrant && !hasCodeResponse)
            return false;

        var hasImplicitGrant = permissions.Contains(OpenIddictConstants.Permissions.GrantTypes.Implicit);
        var hasImplicitResponse = permissions.Any(p =>
            p == OpenIddictConstants.Permissions.ResponseTypes.Token ||
            p == OpenIddictConstants.Permissions.ResponseTypes.IdToken);

        if (hasImplicitGrant && !hasImplicitResponse)
            return false;

        return true;
    }
}