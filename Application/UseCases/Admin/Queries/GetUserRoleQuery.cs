using IbraHabra.NET.Application.Dto;
using IbraHabra.NET.Domain.Constants;
using IbraHabra.NET.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Admin.Queries;

public record GetUserRolesQuery(Guid UserId);

public record UserRoleResponse(Guid RoleId, string RoleName);

public class GetUserRolesHandler : IWolverineHandler
{
    public static async Task<ApiResult<List<UserRoleResponse>>> Handle(
        GetUserRolesQuery query,
        UserManager<User> userManager,
        RoleManager<Role> roleManager)
    {
        var user = await userManager.FindByIdAsync(query.UserId.ToString());
        if (user == null)
            return ApiResult<List<UserRoleResponse>>.Fail(ApiErrors.User.NotFound());

        var roleNames = await userManager.GetRolesAsync(user);
        var roles = new List<UserRoleResponse>();

        foreach (var roleName in roleNames)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role != null)
            {
                roles.Add(new UserRoleResponse(role.Id, role.Name!));
            }
        }

        return ApiResult<List<UserRoleResponse>>.Ok(roles);
    }
}