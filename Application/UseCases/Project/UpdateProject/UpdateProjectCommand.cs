using IbraHabra.NET.Application.Dto.Response;
using IbraHabra.NET.Domain.Entity;
using IbraHabra.NET.Domain.SharedKernel.Interface;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Project.UpdateProject;

public record UpdateProjectCommand(
    Guid Id,
    string DisplayName,
    string Description,
    string HomePageUrl,
    bool AllowRegistration,
    bool AllowSocialLogin) : BaseProjectCommand(DisplayName, Description, HomePageUrl);

public class UpdateProjectHandler : IWolverineHandler
{
    public static async Task<ApiResult> Handle(UpdateProjectCommand command, IRepo<Projects, Guid> repo)
    {
        int result = await repo.UpdateAsync(x => x.Id == command.Id,
            c => c.SetProperty(p => p.DisplayName, command.DisplayName)
                .SetProperty(p => p.Description, command.Description)
                .SetProperty(p => p.HomePageUrl, command.HomePageUrl)
                .SetProperty(p => p.AllowRegistration, command.AllowRegistration)
                .SetProperty(p => p.AllowSocialLogin, command.AllowSocialLogin)
        );
        if (result is 0)
            return ApiResult.Fail(404, "Project not found");

        return ApiResult.Ok();
    }
}