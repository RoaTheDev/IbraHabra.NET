using FluentValidation;

namespace IbraHabra.NET.Application.UseCases.Client.Commands.UpdateClientAuthPolicy;

public class UpdateClientAuthPolicyValidator : AbstractValidator<UpdateClientAuthPolicyCommand>
{
    public UpdateClientAuthPolicyValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty()
            .WithMessage("Client ID is required.");

        RuleFor(x => x.AuthPolicy.MinPasswordLength)
            .InclusiveBetween(6, 128)
            .WithMessage("Minimum password length must be between 6 and 128.");
    }
}