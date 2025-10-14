using IbraHabra.NET.Application.Dto;
using IbraHabra.NET.Domain.Constants;
using IbraHabra.NET.Domain.Contract;
using IbraHabra.NET.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Admin.Commands.AssignProjectRole;

public record AssignProjectRoleCommand(Guid ProjectId, Guid UserId, Guid RoleId);

public class AssignProjectRoleHandler : IWolverineHandler
{
    public static async Task<ApiResult> Handle(
        AssignProjectRoleCommand command,
        IRepo<Projects, Guid> projectRepo,
        IRepo<ProjectRole, Guid> projectRoleRepo,
        IRepo<ProjectMember, ProjectMemberId> memberRepo,
        UserManager<User> userManager,
        IUnitOfWork unitOfWork)
    {
        if (await projectRepo.ExistsAsync(p => p.Id == command.ProjectId))
            return ApiResult.Fail(ApiErrors.Project.NotFound());

        var user = await userManager.FindByIdAsync(command.UserId.ToString());
        if (user == null)
            return ApiResult.Fail(ApiErrors.User.NotFound());

        if (await projectRoleRepo.ExistsAsync(r =>
                r.Id == command.RoleId && r.ProjectId == command.ProjectId))
            return ApiResult.Fail(ApiErrors.ProjectRole.NotFound());

        var memberId = new ProjectMemberId(command.ProjectId, command.UserId);
        var existingMember = await memberRepo.GetViaIdAsync(memberId);

        if (existingMember != null)
        {
            existingMember.ProjectRoleId = command.RoleId;
        }
        else
        {
            var member = new ProjectMember
            {
                ProjectId = command.ProjectId,
                UserId = command.UserId,
                ProjectRoleId = command.RoleId,
                JoinedAt = DateTime.UtcNow
            };
            await memberRepo.AddAsync(member);
        }

        await unitOfWork.SaveChangesAsync();
        return ApiResult.Success();
    }
}