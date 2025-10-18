using IbraHabra.NET.Application.Dto;
using IbraHabra.NET.Domain.Constants;
using IbraHabra.NET.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Admin.Queries;

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
            return ApiResult<List<UserRoleResponse>>.Fail(ApiErrors.ProjectRole.NotFound());

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