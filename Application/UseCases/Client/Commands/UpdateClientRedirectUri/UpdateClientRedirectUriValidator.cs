using FluentValidation;

namespace IbraHabra.NET.Application.UseCases.Client.Commands.UpdateClientRedirectUri;

public class UpdateClientRedirectUrisValidator : AbstractValidator<UpdateClientRedirectUrisCommand>
{
    public UpdateClientRedirectUrisValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty()
            .WithMessage("Client ID is required.");

        RuleForEach(x => x.RedirectUris)
            .Must(BeValidUri)
            .WithMessage("All redirect URIs must be valid absolute URIs.")
            .Must(uri => !uri.Contains("#"))
            .WithMessage("Redirect URIs must not contain fragments (#).");

        RuleForEach(x => x.PostLogoutRedirectUris)
            .Must(BeValidUri)
            .When(x => x.PostLogoutRedirectUris != null)
            .WithMessage("All post-logout redirect URIs must be valid absolute URIs.");
    }

    private bool BeValidUri(string uri)
    {
        return Uri.TryCreate(uri, UriKind.Absolute, out var result) &&
               (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}