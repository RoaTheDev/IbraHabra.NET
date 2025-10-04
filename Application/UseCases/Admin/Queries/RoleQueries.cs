using IbraHabra.NET.Application.Dto.Response;
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

// Get role by ID
public record GetRoleByIdQuery(Guid RoleId);

public class GetRoleByIdHandler : IWolverineHandler
{
    public static async Task<ApiResult<RoleResponse>> Handle(
        GetRoleByIdQuery query,
        RoleManager<Role> roleManager)
    {
        var role = await roleManager.FindByIdAsync(query.RoleId.ToString());
        if (role == null)
            return ApiResult<RoleResponse>.Fail(404, "Role not found.");

        var response = new RoleResponse(role.Id, role.Name!, role.CreatedAt);
        return ApiResult<RoleResponse>.Ok(response);
    }
}

// Get users in a role
public record GetUsersInRoleQuery(Guid RoleId);

public class GetUsersInRoleHandler : IWolverineHandler
{
    public static async Task<ApiResult<List<UserRoleResponse>>> Handle(
        GetUsersInRoleQuery query,
        RoleManager<Role> roleManager,
        UserManager<User> userManager)
    {
        var role = await roleManager.FindByIdAsync(query.RoleId.ToString());
        if (role == null)
            return ApiResult<List<UserRoleResponse>>.Fail(404, "Role not found.");

        var users = await userManager.GetUsersInRoleAsync(role.Name!);
        var response = users.Select(u => new UserRoleResponse(
            u.Id,
            u.Email!,
            u.FirstName,
            u.LastName
        )).ToList();

        return ApiResult<List<UserRoleResponse>>.Ok(response);
    }
}

// Get roles for a user
public record GetUserRolesQuery(Guid UserId);

public class GetUserRolesHandler : IWolverineHandler
{
    public static async Task<ApiResult<List<RoleResponse>>> Handle(
        GetUserRolesQuery query,
        UserManager<User> userManager,
        RoleManager<Role> roleManager)
    {
        var user = await userManager.FindByIdAsync(query.UserId.ToString());
        if (user == null)
            return ApiResult<List<RoleResponse>>.Fail(404, "User not found.");

        var roleNames = await userManager.GetRolesAsync(user);
        var roles = new List<RoleResponse>();

        foreach (var roleName in roleNames)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role != null)
            {
                roles.Add(new RoleResponse(role.Id, role.Name!, role.CreatedAt));
            }
        }

        return ApiResult<List<RoleResponse>>.Ok(roles);
    }
}
