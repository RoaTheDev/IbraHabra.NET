using IbraHabra.NET.Application.Dto.Response;
using IbraHabra.NET.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Admin.Commands.CreateRole;

public record CreateRoleCommand(string RoleName);

public record CreateRoleResponse(Guid RoleId, string RoleName, DateTime CreatedAt);

public class CreateRoleHandler : IWolverineHandler
{
    public static async Task<ApiResult<CreateRoleResponse>> Handle(
        CreateRoleCommand command,
        RoleManager<Role> roleManager)
    {
        // Check if role already exists
        var existingRole = await roleManager.FindByNameAsync(command.RoleName);
        if (existingRole != null)
            return ApiResult<CreateRoleResponse>.Fail(409, $"Role '{command.RoleName}' already exists.");

        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = command.RoleName,
            NormalizedName = command.RoleName.ToUpperInvariant(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await roleManager.CreateAsync(role);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return ApiResult<CreateRoleResponse>.Fail(400, $"Failed to create role: {errors}");
        }

        var response = new CreateRoleResponse(role.Id, role.Name!, role.CreatedAt);
        return ApiResult<CreateRoleResponse>.Ok(response);
    }
}
