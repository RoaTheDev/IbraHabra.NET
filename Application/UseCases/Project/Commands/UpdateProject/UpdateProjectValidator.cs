using FluentValidation;

namespace IbraHabra.NET.Application.UseCases.Project.Commands.UpdateProject;

public class UpdateProjectValidator : AbstractValidator<UpdateProjectCommand>
{
    public UpdateProjectValidator()
    {
        Include(new BaseProjectValidator<UpdateProjectCommand>());
    }
}