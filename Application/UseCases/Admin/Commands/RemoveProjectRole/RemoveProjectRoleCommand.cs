using IbraHabra.NET.Application.Dto;
using IbraHabra.NET.Domain.Constants;
using IbraHabra.NET.Domain.Contract;
using IbraHabra.NET.Domain.Entities;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Admin.Commands.RemoveProjectRole;

public record RemoveProjectRoleCommand(Guid ProjectId, Guid UserId);

public class RemoveProjectRoleHandler : IWolverineHandler
{
    public static async Task<ApiResult> Handle(
        RemoveProjectRoleCommand command,
        IRepo<ProjectMember, ProjectMemberId> memberRepo,
        IUnitOfWork unitOfWork)
    {
        var memberId = new ProjectMemberId(command.ProjectId, command.UserId);
        var member = await memberRepo.GetViaIdAsync(memberId);

        if (member == null)
            return ApiResult.Fail(ApiErrors.ProjectMember.NotFound());

        await memberRepo.DeleteAsync(member.Id);
        await unitOfWork.SaveChangesAsync();

        return ApiResult.Ok();
    }
}