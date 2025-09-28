using FluentValidation;
using IbraHabra.NET.Application.Utils;

namespace IbraHabra.NET.Application.UseCases.Project.CreateProject;

public class CreateProjectValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectValidator()
    {
        Include(new BaseProjectValidator<CreateProjectCommand>());
        RuleFor(x => x.LogoUrl)
            .Must(ValidationUtils.BeValidUrl)
            .When(x => !string.IsNullOrWhiteSpace(x.LogoUrl))
            .WithMessage("The Logo must be a valid URL");
    }
}