using IbraHabra.NET.Application.Dto;
using IbraHabra.NET.Domain.Constants;
using IbraHabra.NET.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Admin.Queries;

// DTOs
public record RoleResponse(Guid RoleId, string RoleName, DateTime CreatedAt);

public record UserRoleResponse(Guid UserId, string Email, string? FirstName, string? LastName);

// Get all roles
public record GetAllRolesQuery();

public class GetAllRolesHandler : IWolverineHandler
{
    public static async Task<ApiResult<List<RoleResponse>>> Handle(
        GetAllRolesQuery query,
        RoleManager<Role> roleManager)
    {
        var roles = await roleManager.Roles
            .Select(r => new RoleResponse(r.Id, r.Name!, r.CreatedAt))
            .ToListAsync();

        return ApiResult<List<RoleResponse>>.Ok(roles);
    }
}

public record GetRoleByIdQuery(Guid RoleId);

public class GetRoleByIdHandler : IWolverineHandler
{
    public static async Task<ApiResult<RoleResponse>> Handle(
        GetRoleByIdQuery query,
        RoleManager<Role> roleManager)
    {
        var role = await roleManager.FindByIdAsync(query.RoleId.ToString());
        if (role == null)
            return ApiResult<RoleResponse>.Fail(ApiErrors.ProjectRole.NotFound());

        var response = new RoleResponse(role.Id, role.Name!, role.CreatedAt);
        return ApiResult<RoleResponse>.Ok(response);
    }
}