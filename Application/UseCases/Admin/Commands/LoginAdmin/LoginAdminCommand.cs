using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IbraHabra.NET.Application.Dto.Response;
using IbraHabra.NET.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
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
        RoleManager<Role> roleManager,
        IConfiguration configuration)
    {
        var user = await userManager.FindByEmailAsync(command.Email);
        if (user == null)
            return ApiResult<LoginAdminCommandResponse>.Fail(401, "Invalid credentials.");

        // Check if user has admin role
        var roles = await userManager.GetRolesAsync(user);
        if (!roles.Contains("Admin") && !roles.Contains("SuperAdmin"))
            return ApiResult<LoginAdminCommandResponse>.Fail(403, "Access denied. Admin privileges required.");

        var result = await signInManager.CheckPasswordSignInAsync(user, command.Password, true);
        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
                return ApiResult<LoginAdminCommandResponse>.Fail(423, "Account locked due to too many failed attempts.");

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

        // Generate JWT token for admin
        var token = await GenerateJwtToken(user, userManager, configuration);
        var expiresAt = DateTime.UtcNow.AddHours(8); // Admin token expires in 8 hours

        return ApiResult<LoginAdminCommandResponse>.Ok(new LoginAdminCommandResponse(
            UserId: user.Id,
            Email: user.Email!,
            Token: token,
            ExpiresAt: expiresAt));
    }

    private static async Task<string> GenerateJwtToken(User user, UserManager<User> userManager, IConfiguration configuration)
    {
        var roles = await userManager.GetRolesAsync(user);
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim()),
            new("admin", "true")
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var jwtSecret = configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"] ?? "IbraHabra",
            audience: configuration["Jwt:Audience"] ?? "IbraHabra.Admin",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
