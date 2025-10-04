using IbraHabra.NET.Application.Dto.Response;
using IbraHabra.NET.Domain.Contract;
using IbraHabra.NET.Domain.Entities;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Project.Queries;

public record GetProjectByIdQuery(Guid ProjectId);

public record GetProjectByIdResponse(
    Guid Id,
    string DisplayName,
    string? Description,
    string? LogoUrl,
    string? HomePageUrl,
    bool AllowRegistration,
    bool AllowSocialLogin,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public class GetProjectByIdHandler : IWolverineHandler
{
    public static async Task<ApiResult<GetProjectByIdResponse>> Handle(GetProjectByIdQuery query,
        IRepo<Projects, Guid> repo)
    {
        var project = await repo.GetViaIdAsync(query.ProjectId);

        if (project == null)
            return ApiResult<GetProjectByIdResponse>.Fail(404, "Project not found.");

        return ApiResult<GetProjectByIdResponse>.Ok(new GetProjectByIdResponse(
            project.Id,
            project.DisplayName,
            project.Description,
            project.LogoUrl,
            project.HomePageUrl,
            project.AllowRegistration,
            project.AllowSocialLogin,
            project.IsActive,
            project.CreatedAt,
            project.UpdatedAt));
    }
}