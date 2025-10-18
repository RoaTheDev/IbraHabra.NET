using IbraHabra.NET.Application.Dto;
using IbraHabra.NET.Domain.Contract;
using IbraHabra.NET.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Admin.Queries;

public record GetProjectMembersQuery(Guid ProjectId);
public record ProjectMemberResponse(
    Guid UserId,
    string Email,
    string? FirstName,
    string? LastName,
    Guid RoleId,
    string RoleName,
    DateTime JoinedAt);
public class GetProjectMembersHandler : IWolverineHandler
{
    public static async Task<ApiResult<List<ProjectMemberResponse>>> Handle(
        GetProjectMembersQuery query,
        IRepo<ProjectMember, ProjectMemberId> memberRepo)
    {
        var members = await memberRepo.GetAllViaConditionAsync(
            m => m.ProjectId == query.ProjectId,
            q => q.Include(m => m.User)
                .Include(m => m.ProjectRole));

        var response = members.Select(m => new ProjectMemberResponse(
            m.UserId,
            m.User.Email!,
            m.User.FirstName,
            m.User.LastName,
            m.ProjectRoleId,
            m.ProjectRole.Name,
            m.JoinedAt
        )).ToList();

        return ApiResult<List<ProjectMemberResponse>>.Ok(response);
    }
}