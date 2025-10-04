using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IbraHabra.NET.Application.Dto.Response;
using IbraHabra.NET.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
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
        SignInManager<User> signInManager,
        IConfiguration configuration)
    {
        var user = await userManager.FindByEmailAsync(command.Email);
        if (user == null)
            return ApiResult<Verify2FaAdminCommandResponse>.Fail(401, "Invalid credentials.");

        // Check if user has admin role
        var roles = await userManager.GetRolesAsync(user);
        if (!roles.Contains("Admin") && !roles.Contains("SuperAdmin"))
            return ApiResult<Verify2FaAdminCommandResponse>.Fail(403, "Access denied. Admin privileges required.");

        // Verify 2FA code
        var isValid = await userManager.VerifyTwoFactorTokenAsync(
            user,
            userManager.Options.Tokens.AuthenticatorTokenProvider,
            command.Code);

        if (!isValid)
            return ApiResult<Verify2FaAdminCommandResponse>.Fail(401, "Invalid 2FA code.");

        // Sign in the user
        await signInManager.SignInAsync(user, isPersistent: false);

        // Generate JWT token
        var token = await GenerateJwtToken(user, userManager, configuration);
        var expiresAt = DateTime.UtcNow.AddHours(8);

        return ApiResult<Verify2FaAdminCommandResponse>.Ok(new Verify2FaAdminCommandResponse(
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
