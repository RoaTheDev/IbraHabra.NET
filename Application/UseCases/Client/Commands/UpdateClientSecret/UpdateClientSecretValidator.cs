using FluentValidation;

namespace IbraHabra.NET.Application.UseCases.Client.Commands.UpdateClientSecret;

public class UpdateClientSecretValidator : AbstractValidator<UpdateClientSecretCommand>
{
    public UpdateClientSecretValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty()
            .WithMessage("Client ID is required.");

        RuleFor(x => x.NewClientSecret)
            .NotEmpty()
            .WithMessage("Client secret is required.")
            .MinimumLength(32)
            .WithMessage("Client secret must be at least 32 characters for security.")
            .Matches(@"^[a-zA-Z0-9_\-\.!@#$%^&*()+=]+$")
            .WithMessage("Client secret contains invalid characters.");
    }
}