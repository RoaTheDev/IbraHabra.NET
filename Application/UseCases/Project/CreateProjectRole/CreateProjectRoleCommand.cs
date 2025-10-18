using IbraHabra.NET.Application.Dto;
using IbraHabra.NET.Domain.Constants;
using IbraHabra.NET.Domain.Contract;
using IbraHabra.NET.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Project.CreateProjectRole;

public record CreateProjectRoleCommand(
    Guid ProjectId,
    string Name,
    string Description,
    List<Guid>? PermissionIds);

public record CreateProjectRoleResponse(Guid ProjectRoleId, Guid ProjectId);

public class CreateProjectRoleHandler : IWolverineHandler
{
    public static async Task<ApiResult<CreateProjectRoleResponse>> Handle(
        CreateProjectRoleCommand command,
        IRepo<Projects, Guid> projectRepo,
        IRepo<ProjectRole, Guid> roleRepo,
        IRepo<Permission, Guid> permissionRepo,
        IUnitOfWork unitOfWork)
    {
        if (await projectRepo.ExistsAsync(p => p.Id == command.ProjectId))
            return ApiResult<CreateProjectRoleResponse>.Fail(ApiErrors.Project.NotFound());

        var existingRole =
            await roleRepo.GetViaConditionAsync(r => r.ProjectId == command.ProjectId && r.Name == command.Name);
        if (existingRole != null)
            return ApiResult<CreateProjectRoleResponse>.Fail(ApiErrors.Project.DuplicateName());


        var context = unitOfWork.DbContext;
        var strategy = context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                var projectRole = new ProjectRole
                {
                    Id = Guid.NewGuid(),
                    Name = command.Name,
                    Description = command.Description,
                    ProjectId = command.ProjectId
                };
                await roleRepo.AddAsync(projectRole);

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
                await transaction.CommitAsync();
                var response = new CreateProjectRoleResponse(projectRole.Id, projectRole.ProjectId);
                return ApiResult<CreateProjectRoleResponse>.Ok(response);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }
}