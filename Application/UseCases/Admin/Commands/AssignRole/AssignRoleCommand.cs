using IbraHabra.NET.Application.Dto;
using IbraHabra.NET.Domain.Constants;
using IbraHabra.NET.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Admin.Commands.AssignRole;

public record AssignRoleCommand(Guid UserId, string RoleName);

public class AssignRoleHandler : IWolverineHandler
{
    public static async Task<ApiResult> Handle(
        AssignRoleCommand command,
        UserManager<User> userManager,
        RoleManager<Role> roleManager)
    {
        var user = await userManager.FindByIdAsync(command.UserId.ToString());
        if (user == null)
            return ApiResult.Fail(ApiErrors.User.NotFound());

        var roleExists = await roleManager.RoleExistsAsync(command.RoleName);
        if (!roleExists)
            return ApiResult.Fail(ApiErrors.Authorization.RoleNotFound());

        var isInRole = await userManager.IsInRoleAsync(user, command.RoleName);
        if (isInRole)
            return ApiResult.Fail(ApiErrors.Authorization.RoleAlreadyAssigned());

        var result = await userManager.AddToRoleAsync(user, command.RoleName);
        return !result.Succeeded ? ApiResult.Fail(ApiErrors.Common.ServerError()) : ApiResult.Ok();
    }
}