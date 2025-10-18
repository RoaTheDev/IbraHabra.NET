using IbraHabra.NET.Application.Dto;
using IbraHabra.NET.Domain.Constants;
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
            return ApiResult.Fail(ApiErrors.ProjectRole.NotFound());

        if (!string.IsNullOrWhiteSpace(command.Name) && command.Name != role.Name)
        {
            var existingRole = await roleRepo.GetViaConditionAsync(r =>
                r.ProjectId == command.ProjectId && r.Name == command.Name && r.Id != command.RoleId);
            if (existingRole != null)
                return ApiResult.Fail(ApiErrors.ProjectRole.DuplicateName());

            role.Name = command.Name;
        }

        if (command.Description != null)
        {
            role.Description = command.Description;
        }

        if (command.PermissionIds != null)
        {
            role.ProjectRolePermissions.Clear();

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