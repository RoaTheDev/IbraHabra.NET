using IbraHabra.NET.Application.Dto;
using IbraHabra.NET.Application.Dto.Response;
using IbraHabra.NET.Domain.Constants;
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




