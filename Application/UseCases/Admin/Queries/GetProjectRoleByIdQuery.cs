using IbraHabra.NET.Application.Dto;
using IbraHabra.NET.Domain.Constants;
using IbraHabra.NET.Domain.Contract;
using IbraHabra.NET.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Admin.Queries;

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
            return ApiResult<ProjectRoleResponse>.Fail(ApiErrors.ProjectRole.NotFound());

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