using IbraHabra.NET.Application.Dto.Response;
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
            return ApiResult.Fail(404, "User not found.");

        var roleExists = await roleManager.RoleExistsAsync(command.RoleName);
        if (!roleExists)
            return ApiResult.Fail(404, $"Role '{command.RoleName}' does not exist.");

        var isInRole = await userManager.IsInRoleAsync(user, command.RoleName);
        if (isInRole)
            return ApiResult.Fail(409, $"User already has the '{command.RoleName}' role.");

        var result = await userManager.AddToRoleAsync(user, command.RoleName);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return ApiResult.Fail(400, $"Failed to assign role: {errors}");
        }

        return ApiResult.Ok();
    }
}
