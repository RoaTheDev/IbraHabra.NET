using IbraHabra.NET.Application.Dto.Response;
using IbraHabra.NET.Domain.Contract;
using IbraHabra.NET.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Admin.Commands.DeleteProjectRole;

public record DeleteProjectRoleCommand(Guid ProjectId, Guid RoleId);

public class DeleteProjectRoleHandler : IWolverineHandler
{
    public static async Task<ApiResult> Handle(
        DeleteProjectRoleCommand command,
        IRepo<ProjectRole, Guid> roleRepo,
        IRepo<ProjectMember, ProjectMemberId> memberRepo,
        IUnitOfWork unitOfWork)
    {
        var role = await roleRepo.GetViaConditionAsync(
            r => r.Id == command.RoleId && r.ProjectId == command.ProjectId,
            query => query.Include(r => r.ProjectMembers));

        if (role == null)
            return ApiResult.Fail(404, "Project role not found.");

        if (role.ProjectMembers.Any())
            return ApiResult.Fail(409,
                $"Cannot delete role. It is assigned to {role.ProjectMembers.Count} member(s). " +
                "Please reassign or remove these members first.");

        await roleRepo.DeleteAsync(role.Id);

        await unitOfWork.SaveChangesAsync();

        return ApiResult.Ok();
    }
}