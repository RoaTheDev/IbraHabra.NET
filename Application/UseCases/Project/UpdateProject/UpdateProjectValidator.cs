using FluentValidation;
using IbraHabra.NET.Application.Dto.Request.Project;

namespace IbraHabra.NET.Application.UseCases.Project.UpdateProject;

public class UpdateProjectValidator : AbstractValidator<UpdateProjectCommand>
{
    public UpdateProjectValidator()
    {
        Include(new BaseProjectValidator<UpdateProjectCommand>());
    }
}