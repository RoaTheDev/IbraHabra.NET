using IbraHabra.NET.Application.Dto.Response;
using IbraHabra.NET.Application.Utils;
using IbraHabra.NET.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Admin.Commands.LoginAdmin;

public record LoginAdminCommand(string Email, string Password);

public record LoginAdminCommandResponse(
    Guid UserId,
    string Email,
    string Token,
    DateTime ExpiresAt,
    bool RequiresTwoFactor = false,
    string? TwoFactorToken = null);

public class LoginAdminHandler : IWolverineHandler
{
    public static async Task<ApiResult<LoginAdminCommandResponse>> Handle(
        LoginAdminCommand command,
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IConfiguration configuration)
    {
        var user = await userManager.FindByEmailAsync(command.Email);
        if (user == null)
            return ApiResult<LoginAdminCommandResponse>.Fail(401, "Invalid credentials.");

        // Check if user has admin role
        var roles = await userManager.GetRolesAsync(user);
        Console.WriteLine(string.Join(", ", roles));
        if (!roles.Contains("Admin") && !roles.Contains("Super"))
            return ApiResult<LoginAdminCommandResponse>.Fail(403, "Access denied. Admin privileges required.");

        var result = await signInManager.CheckPasswordSignInAsync(user, command.Password, true);
        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
                return ApiResult<LoginAdminCommandResponse>.Fail(423,
                    "Account locked due to too many failed attempts.");

            return ApiResult<LoginAdminCommandResponse>.Fail(401, "Invalid credentials.");
        }

        if (result.RequiresTwoFactor)
        {
            // For admin, we'll use a simple temporary token approach
            var twoFactorToken = Guid.NewGuid().ToString();

            return ApiResult<LoginAdminCommandResponse>.Ok(202, new LoginAdminCommandResponse(
                UserId: user.Id,
                Email: user.Email!,
                Token: string.Empty,
                ExpiresAt: DateTime.UtcNow,
                RequiresTwoFactor: true,
                TwoFactorToken: twoFactorToken));
        }

        var token = await JwtGen.GenerateJwtToken(user, userManager, configuration);
        var expiresAt = DateTime.UtcNow.AddHours(8); 

        return ApiResult<LoginAdminCommandResponse>.Ok(new LoginAdminCommandResponse(
            UserId: user.Id,
            Email: user.Email!,
            Token: token,
            ExpiresAt: expiresAt));
    }
}

