using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IbraHabra.NET.Application.Dto.Response;
using IbraHabra.NET.Domain.Contract.Services;
using IbraHabra.NET.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Admin.Commands.RefreshAdminToken;

public record RefreshAdminTokenCommand(string Token);

public record RefreshAdminTokenResponse(
    string Token,
    DateTime ExpiresAt);

public class RefreshAdminTokenHandler : IWolverineHandler
{
    public static async Task<ApiResult<RefreshAdminTokenResponse>> Handle(
        RefreshAdminTokenCommand command,
        ICurrentUserService currentUserService,
        UserManager<User> userManager,
        IConfiguration configuration)
    {
        var userId = currentUserService.UserId;
        if (userId == Guid.Empty)
            return ApiResult<RefreshAdminTokenResponse>.Fail(401, "Unauthorized.");

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return ApiResult<RefreshAdminTokenResponse>.Fail(404, "User not found.");

        // Verify user still has admin role
        var roles = await userManager.GetRolesAsync(user);
        if (!roles.Contains("Admin") && !roles.Contains("SuperAdmin"))
            return ApiResult<RefreshAdminTokenResponse>.Fail(403, "Access denied. Admin privileges required.");

        // Generate new JWT token
        var token = await GenerateJwtToken(user, userManager, configuration);
        var expiresAt = DateTime.UtcNow.AddHours(8);

        return ApiResult<RefreshAdminTokenResponse>.Ok(new RefreshAdminTokenResponse(
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
