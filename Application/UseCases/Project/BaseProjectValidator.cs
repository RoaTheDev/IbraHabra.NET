using FluentValidation;
using IbraHabra.NET.Application.Utils;

namespace IbraHabra.NET.Application.UseCases.Project;

public class BaseProjectValidator<T> : AbstractValidator<T> where T : BaseProjectCommand
{
    public BaseProjectValidator()
    {
        RuleFor(x => x.DisplayName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Display name cannot be null or empty.")
            .MaximumLength(50).WithMessage("Display name cannot be over 50 chars.");

        RuleFor(x => x.Description)
            .Cascade(CascadeMode.Stop)
            .MaximumLength(255).WithMessage("Description cannot be over 255 chars.");

        RuleFor(x => x.HomePageUrl)
            .Cascade(CascadeMode.Stop)
            .Must(ValidationUtils.BeValidUrl).When(x => !string.IsNullOrWhiteSpace(x.HomePageUrl))
            .WithMessage("The Home URL must be a valid URL")
            .MaximumLength(255).WithMessage("The Home URL cannot be over 255 chars.");
    }
}