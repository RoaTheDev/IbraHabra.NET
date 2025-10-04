using IbraHabra.NET.Application.Dto.Response;
using IbraHabra.NET.Domain.Contract;
using IbraHabra.NET.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Admin.Queries;

// DTOs
public record ProjectRoleResponse(
    Guid RoleId,
    string Name,
    string Description,
    Guid ProjectId,
    List<PermissionResponse> Permissions);

public record PermissionResponse(Guid PermissionId, string Name);

public record ProjectMemberResponse(
    Guid UserId,
    string Email,
    string? FirstName,
    string? LastName,
    Guid RoleId,
    string RoleName,
    DateTime JoinedAt);

public record GetProjectRolesQuery(Guid ProjectId);

public class GetProjectRolesHandler : IWolverineHandler
{
    public static async Task<ApiResult<List<ProjectRoleResponse>>> Handle(
        GetProjectRolesQuery query,
        IRepo<ProjectRole, Guid> roleRepo)
    {
        var roles = await roleRepo.GetAllViaConditionAsync(
            r => r.ProjectId == query.ProjectId,
            q => q.Include(r => r.ProjectRolePermissions)
                .ThenInclude(prp => prp.Permission));

        var response = roles.Select(r => new ProjectRoleResponse(
            r.Id,
            r.Name,
            r.Description,
            r.ProjectId,
            r.ProjectRolePermissions.Select(prp => new PermissionResponse(
                prp.PermissionId,
                prp.Permission.Name
            )).ToList()
        )).ToList();

        return ApiResult<List<ProjectRoleResponse>>.Ok(response);
    }
}

public record GetProjectRoleByIdQuery(Guid ProjectId, Guid RoleId);

public class GetProjectRoleByIdHandler : IWolverineHandler
{
    public static async Task<ApiResult<ProjectRoleResponse>> Handle(
        GetProjectRoleByIdQuery query,
        IRepo<ProjectRole, Guid> roleRepo)
    {
        var role = await roleRepo.GetViaConditionAsync(
            r => r.Id == query.RoleId && r.ProjectId == query.ProjectId,
            q => q.Include(r => r.ProjectRolePermissions)
                .ThenInclude(prp => prp.Permission));

        if (role == null)
            return ApiResult<ProjectRoleResponse>.Fail(404, "Project role not found.");

        var response = new ProjectRoleResponse(
            role.Id,
            role.Name,
            role.Description,
            role.ProjectId,
            role.ProjectRolePermissions.Select(prp => new PermissionResponse(
                prp.PermissionId,
                prp.Permission.Name
            )).ToList()
        );

        return ApiResult<ProjectRoleResponse>.Ok(response);
    }
}

public record GetProjectMembersQuery(Guid ProjectId);

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