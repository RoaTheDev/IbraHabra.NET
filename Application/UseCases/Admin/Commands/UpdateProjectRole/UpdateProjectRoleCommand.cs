using IbraHabra.NET.Application.Dto.Response;
using IbraHabra.NET.Domain.Contract;
using IbraHabra.NET.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Admin.Commands.UpdateProjectRole;

public record UpdateProjectRoleCommand(
    Guid ProjectId,
    Guid RoleId,
    string? Name,
    string? Description,
    List<Guid>? PermissionIds);

public class UpdateProjectRoleHandler : IWolverineHandler
{
    public static async Task<ApiResult> Handle(
        UpdateProjectRoleCommand command,
        IRepo<ProjectRole, Guid> roleRepo,
        IRepo<Permission, Guid> permissionRepo,
        IUnitOfWork unitOfWork)
    {
        var role = await roleRepo.GetViaConditionAsync(
            r => r.Id == command.RoleId && r.ProjectId == command.ProjectId,
            query => query.Include(r => r.ProjectRolePermissions));

        if (role == null)
            return ApiResult.Fail(404, "Project role not found.");

        // Update name if provided
        if (!string.IsNullOrWhiteSpace(command.Name) && command.Name != role.Name)
        {
            // Check if new name already exists
            var existingRole = await roleRepo.GetViaConditionAsync(
                r => r.ProjectId == command.ProjectId && r.Name == command.Name && r.Id != command.RoleId);
            if (existingRole != null)
                return ApiResult.Fail(409, $"Role '{command.Name}' already exists in this project.");

            role.Name = command.Name;
        }

        // Update description if provided
        if (command.Description != null)
        {
            role.Description = command.Description;
        }

        // Update permissions if provided
        if (command.PermissionIds != null)
        {
            // Remove existing permissions
            role.ProjectRolePermissions.Clear();

            // Add new permissions
            foreach (var permissionId in command.PermissionIds)
            {
                var permission = await permissionRepo.GetViaIdAsync(permissionId);
                if (permission != null)
                {
                    role.ProjectRolePermissions.Add(new ProjectRolePermission
                    {
                        ProjectRoleId = role.Id,
                        PermissionId = permissionId
                    });
                }
            }
        }

        await unitOfWork.SaveChangesAsync();
        return ApiResult.Ok();
    }
}
