using IbraHabra.NET.Application.Dto.Response;
using IbraHabra.NET.Domain.Contract;
using IbraHabra.NET.Domain.Entities;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Project.Commands.CreateProject;

public record CreateProjectCommand(
    string DisplayName,
    string? Description,
    string? HomePageUrl,
    string? LogoUrl,
    bool AllowRegistration,
    bool AllowSocialLogin)
    : BaseProjectCommand(DisplayName, Description, HomePageUrl);

public record CreateProjectResponse(Guid Id);

public class CreateProjectHandler : IWolverineHandler
{
    public static async Task<ApiResult<CreateProjectResponse>> Handle(CreateProjectCommand command,
        IRepo<Projects, Guid> repo, IUnitOfWork unitOfWork)
    {
        var project = new Projects
        {
            Id = Guid.CreateVersion7(),
            DisplayName = command.DisplayName,
            Description = command.Description,
            LogoUrl = command.LogoUrl,
            HomePageUrl = command.HomePageUrl,
            AllowRegistration = command.AllowRegistration,
            AllowSocialLogin = command.AllowSocialLogin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        await repo.AddAsync(project);
        await unitOfWork.SaveChangesAsync();
        return ApiResult<CreateProjectResponse>.Ok(new(project.Id));
    }
}