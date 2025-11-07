using FluentValidation;
using IbraHabra.NET.Domain.Constants;

namespace IbraHabra.NET.Application.UseCases.Client.Commands.UpdateClientMetadata;

public class UpdateClientMetadataValidator : AbstractValidator<UpdateClientMetadataCommand>
{
    public UpdateClientMetadataValidator()
    {
        RuleFor(x => x)
            .Must(x => !string.IsNullOrEmpty(x.DisplayName) ||
                       !string.IsNullOrEmpty(x.ApplicationType) ||
                       !string.IsNullOrEmpty(x.ConsentType))
            .WithMessage("At least one field must be provided for update.");

        RuleFor(x => x.ClientId)
            .NotEmpty()
            .WithMessage("Client ID is required.");

        RuleFor(x => x.DisplayName)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.DisplayName))
            .WithMessage("Display name must not exceed 200 characters.");

        RuleFor(x => x.ApplicationType)
            .Must(type => OauthConstantValidation.ValidApplicationTypes.Contains(type!))
            .When(x => !string.IsNullOrEmpty(x.ApplicationType))
            .WithMessage(
                $"Application type must be one of: {string.Join(", ", OauthConstantValidation.ValidApplicationTypes)}");

        RuleFor(x => x.ConsentType)
            .Must(type => OauthConstantValidation.ValidConsentTypes.Contains(type!))
            .When(x => !string.IsNullOrEmpty(x.ConsentType))
            .WithMessage(
                $"Consent type must be one of: {string.Join(", ", OauthConstantValidation.ValidConsentTypes)}");

        RuleFor(x => x)
            .Must(x => !string.IsNullOrEmpty(x.DisplayName) ||
                       !string.IsNullOrEmpty(x.ApplicationType) ||
                       !string.IsNullOrEmpty(x.ConsentType))
            .WithMessage("At least one field must be provided for update.");
    }
}