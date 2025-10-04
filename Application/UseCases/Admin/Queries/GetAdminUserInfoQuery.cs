using IbraHabra.NET.Application.Dto.Response;
using IbraHabra.NET.Domain.Contract.Services;
using IbraHabra.NET.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Admin.Queries;

public record GetAdminUserInfoQuery();

public record AdminUserInfoResponse(
    Guid UserId,
    string Email,
    string? FirstName,
    string? LastName,
    List<string> Roles,
    bool EmailConfirmed,
    bool TwoFactorEnabled,
    DateTime CreatedAt);

public class GetAdminUserInfoHandler : IWolverineHandler
{
    public static async Task<ApiResult<AdminUserInfoResponse>> Handle(
        GetAdminUserInfoQuery query,
        ICurrentUserService currentUserService,
        UserManager<User> userManager)
    {
        var userId = currentUserService.UserId;
        if (userId == Guid.Empty)
            return ApiResult<AdminUserInfoResponse>.Fail(401, "Unauthorized.");

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return ApiResult<AdminUserInfoResponse>.Fail(404, "User not found.");

        var roles = await userManager.GetRolesAsync(user);

        var response = new AdminUserInfoResponse(
            UserId: user.Id,
            Email: user.Email!,
            FirstName: user.FirstName,
            LastName: user.LastName,
            Roles: roles.ToList(),
            EmailConfirmed: user.EmailConfirmed,
            TwoFactorEnabled: user.TwoFactorEnabled,
            CreatedAt: user.CreatedAt);

        return ApiResult<AdminUserInfoResponse>.Ok(response);
    }
}
