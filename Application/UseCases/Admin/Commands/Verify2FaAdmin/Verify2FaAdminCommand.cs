using IbraHabra.NET.Application.Dto;
using IbraHabra.NET.Application.Utils;
using IbraHabra.NET.Domain.Constants;
using IbraHabra.NET.Domain.Contract;
using IbraHabra.NET.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Admin.Commands.Verify2FaAdmin;

public record Verify2FaAdminCommand(string Email, string Code);

public record Verify2FaAdminCommandResponse(
    Guid UserId,
    string Email,
    string Token,
    DateTime ExpiresAt);

public class Verify2FaAdminHandler : IWolverineHandler
{
    public static async Task<ApiResult<Verify2FaAdminCommandResponse>> Handle(
        Verify2FaAdminCommand command,
        UserManager<User> userManager,
        SignInManager<User> signInManager, IOptions<JwtOptions> jwtOptions)
    {
        var user = await userManager.FindByEmailAsync(command.Email);
        if (user == null)
            return ApiResult<Verify2FaAdminCommandResponse>.Fail(ApiErrors.User.NotFound());

        var roles = await userManager.GetRolesAsync(user);
        if (!roles.Contains("Admin") && !roles.Contains("SuperAdmin"))
            return ApiResult<Verify2FaAdminCommandResponse>.Fail(ApiErrors.Authorization.InsufficientPermissions());

        var isValid = await userManager.VerifyTwoFactorTokenAsync(
            user,
            userManager.Options.Tokens.AuthenticatorTokenProvider,
            command.Code);

        if (!isValid)
            return ApiResult<Verify2FaAdminCommandResponse>.Fail(ApiErrors.User.InvalidTwoFactorCode());

        await signInManager.SignInAsync(user, isPersistent: false);

        var token = await JwtGen.GenerateJwtToken(user, userManager, jwtOptions);
        var expiresAt = DateTime.UtcNow.AddHours(8);

        return ApiResult<Verify2FaAdminCommandResponse>.Ok(new Verify2FaAdminCommandResponse(
            UserId: user.Id,
            Email: user.Email!,
            Token: token,
            ExpiresAt: expiresAt));
    }
}