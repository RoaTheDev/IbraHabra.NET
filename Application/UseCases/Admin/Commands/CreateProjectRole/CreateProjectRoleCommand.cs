using IbraHabra.NET.Application.Dto.Response;
using IbraHabra.NET.Domain.Contract;
using IbraHabra.NET.Domain.Entities;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Admin.Commands.CreateProjectRole;

public record CreateProjectRoleCommand(
    Guid ProjectId,
    string Name,
    string Description,
    List<Guid>? PermissionIds);

public record CreateProjectRoleResponse(Guid RoleId, string Name, Guid ProjectId);

public class CreateProjectRoleHandler : IWolverineHandler
{
    public static async Task<ApiResult<CreateProjectRoleResponse>> Handle(
        CreateProjectRoleCommand command,
        IRepo<Projects, Guid> projectRepo,
        IRepo<ProjectRole, Guid> roleRepo,
        IRepo<Permission, Guid> permissionRepo,
        IUnitOfWork unitOfWork)
    {
        // Verify project exists
        var project = await projectRepo.GetViaIdAsync(command.ProjectId);
        if (project == null)
            return ApiResult<CreateProjectRoleResponse>.Fail(404, "Project not found.");

        // Check if role name already exists in this project
        var existingRole = await roleRepo.GetViaConditionAsync(
            r => r.ProjectId == command.ProjectId && r.Name == command.Name);
        if (existingRole != null)
            return ApiResult<CreateProjectRoleResponse>.Fail(409, 
                $"Role '{command.Name}' already exists in this project.");

        var projectRole = new ProjectRole
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Description = command.Description,
            ProjectId = command.ProjectId
        };

        await roleRepo.AddAsync(projectRole);

        // Add permissions if provided
        if (command.PermissionIds != null && command.PermissionIds.Any())
        {
            foreach (var permissionId in command.PermissionIds)
            {
                var permission = await permissionRepo.GetViaIdAsync(permissionId);
                if (permission != null)
                {
                    projectRole.ProjectRolePermissions.Add(new ProjectRolePermission
                    {
                        ProjectRoleId = projectRole.Id,
                        PermissionId = permissionId
                    });
                }
            }
        }

        await unitOfWork.SaveChangesAsync();

        var response = new CreateProjectRoleResponse(projectRole.Id, projectRole.Name, projectRole.ProjectId);
        return ApiResult<CreateProjectRoleResponse>.Ok(response);
    }
}
