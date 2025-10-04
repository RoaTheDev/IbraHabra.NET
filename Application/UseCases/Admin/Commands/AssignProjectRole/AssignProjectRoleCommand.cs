using IbraHabra.NET.Application.Dto.Response;
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
        IRepo<ProjectRole, Guid> roleRepo,
        IRepo<ProjectMember, ProjectMemberId> memberRepo,
        UserManager<User> userManager,
        IUnitOfWork unitOfWork)
    {
        var project = await projectRepo.GetViaIdAsync(command.ProjectId);
        if (project == null)
            return ApiResult.Fail(404, "Project not found.");

        var user = await userManager.FindByIdAsync(command.UserId.ToString());
        if (user == null)
            return ApiResult.Fail(404, "User not found.");

        var role = await roleRepo.GetViaConditionAsync(
            r => r.Id == command.RoleId && r.ProjectId == command.ProjectId);
        if (role == null)
            return ApiResult.Fail(404, "Project role not found.");

        var memberId = new ProjectMemberId(command.ProjectId, command.UserId);
        var existingMember = await memberRepo.GetViaIdAsync(memberId);
        
        if (existingMember != null)
        {
            // Update the role
            existingMember.ProjectRoleId = command.RoleId;
        }
        else
        {
            // Add new member
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
        return ApiResult.Ok();
    }
}
