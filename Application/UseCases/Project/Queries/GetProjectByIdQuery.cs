using IbraHabra.NET.Application.Dto;
using IbraHabra.NET.Application.Dto.Response;
using IbraHabra.NET.Domain.Constants;
using IbraHabra.NET.Domain.Contract;
using IbraHabra.NET.Domain.Entities;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Project.Queries;

public record GetProjectByIdQuery(Guid Id);

public record GetProjectByIdResponse(
    Guid Id,
    string DisplayName,
    string? Description,
    string? LogoUrl,
    string? HomePageUrl,
    bool AllowRegistration,
    bool AllowSocialLogin,
    bool IsActive,
    List<ClientSummary> Clients,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public class GetProjectByIdHandler : IWolverineHandler
{
    public static async Task<ApiResult<GetProjectByIdResponse>> Handle(GetProjectByIdQuery query,
        IRepo<Projects, Guid> repo)
    {
        var project = await repo.GetViaConditionAsync(
            p => p.Id == query.Id,
            p => new GetProjectByIdResponse(
                p.Id,
                p.DisplayName,
                p.Description,
                p.LogoUrl,
                p.HomePageUrl,
                p.AllowRegistration,
                p.AllowSocialLogin,
                p.IsActive,
                p.OauthApplications.Select(c => new ClientSummary(
                    c.ClientId!,
                    c.ProjectId,
                    c.DisplayName,
                    c.ApplicationType,
                    c.ClientType,
                    c.ConsentType,
                    c.IsActive,
                    c.CreatedAt,
                    c.UpdatedAt
                )).ToList(),
                p.CreatedAt,
                p.UpdatedAt
            )
        );
        if (project == null)
            return ApiResult<GetProjectByIdResponse>.Fail(ApiErrors.Project.NotFound());

        return ApiResult<GetProjectByIdResponse>.Ok(project);
    }
}