using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IbraHabra.NET.Application.Dto.Response;
using IbraHabra.NET.Application.Utils;
using IbraHabra.NET.Domain.Entities;
using Microsoft.AspNetCore.Identity;
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
        UserManager<User> userManager,
        IConfiguration configuration)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtSecret = configuration["JWT:SECRET"] ?? Environment.GetEnvironmentVariable("JWT_SECRET");
        
        if (string.IsNullOrEmpty(jwtSecret))
            return ApiResult<RefreshAdminTokenResponse>.Fail(500, "Server configuration error.");

        var key = Encoding.UTF8.GetBytes(jwtSecret);

        try
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = configuration["JWT:ISSUER"] ?? "IbraHabra",
                ValidateAudience = true,
                ValidAudience = configuration["JWT:AUDIENCE"] ?? "IbraHabra.Domain.Coordinator",
                ValidateLifetime = false, 
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(command.Token, validationParameters, out _);
            
            // Extract user ID from token
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return ApiResult<RefreshAdminTokenResponse>.Fail(401, "Invalid token.");

            var user = await userManager.FindByIdAsync(userIdClaim.Value);
            if (user == null)
                return ApiResult<RefreshAdminTokenResponse>.Fail(404, "User not found.");

            // Verify user still has admin role
            var roles = await userManager.GetRolesAsync(user);
            if (!roles.Contains("Admin") && !roles.Contains("Super"))
                return ApiResult<RefreshAdminTokenResponse>.Fail(403, "Access denied.");

            // Generate new token
            var newToken = await JwtGen.GenerateJwtToken(user, userManager, configuration);
            var expiresAt = DateTime.UtcNow.AddHours(8);

            return ApiResult<RefreshAdminTokenResponse>.Ok(new RefreshAdminTokenResponse(
                Token: newToken,
                ExpiresAt: expiresAt));
        }
        catch (SecurityTokenException)
        {
            return ApiResult<RefreshAdminTokenResponse>.Fail(401, "Invalid token.");
        }
    }

}