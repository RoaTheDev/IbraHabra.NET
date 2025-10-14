using IbraHabra.NET.Application.Dto;
using IbraHabra.NET.Domain.Constants;
using IbraHabra.NET.Domain.Contract;
using IbraHabra.NET.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Project.DeleteProjectRole;

public record DeleteProjectRoleCommand(Guid ProjectId, Guid RoleId);

public class DeleteProjectRoleHandler : IWolverineHandler
{
    public static async Task<ApiResult> Handle(
        DeleteProjectRoleCommand command,
        IRepo<ProjectRole, Guid> roleRepo,
        IUnitOfWork unitOfWork)
    {
        var role = await roleRepo.GetViaConditionAsync(
            r => r.Id == command.RoleId && r.ProjectId == command.ProjectId,
            query => query.Include(r => r.ProjectMembers));

        if (role == null)
            return ApiResult.Fail(ApiErrors.Project.NotFound());

        if (role.ProjectMembers.Any())
            return ApiResult.Fail(ApiErrors.ProjectRole.CannotDeleteRoleWithMembers());

        await roleRepo.DeleteAsync(role.Id);

        await unitOfWork.SaveChangesAsync();

        return ApiResult.Success();
    }
}