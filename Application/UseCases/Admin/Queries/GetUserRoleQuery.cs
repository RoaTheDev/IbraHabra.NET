using IbraHabra.NET.Application.Dto;
using IbraHabra.NET.Domain.Constants;
using IbraHabra.NET.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Admin.Queries;

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
            return ApiResult<List<RoleResponse>>.Fail(ApiErrors.User.NotFound());

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