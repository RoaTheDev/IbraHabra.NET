using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IbraHabra.NET.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace IbraHabra.NET.Application.Utils;

public class JwtGen
{
    public static async Task<string> GenerateJwtToken(User user, UserManager<User> userManager,
        IConfiguration configuration)
    {
        var roles = await userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(JwtRegisteredClaimNames.Name, $"{user.FirstName} {user.LastName}".Trim()),
            new("role", string.Join(", ", roles))
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var jwtSecret = configuration["JWT:SECRET"] ?? Environment.GetEnvironmentVariable("JWT_SECRET")
            ?? throw new InvalidOperationException("JWT Secret not configured");

        var issuer = configuration["JWT:ISSUER"] ?? "IbraHabra";
        var audience = configuration["JWT:AUDIENCE"] ?? "IbraHabra.Domain.Coordinator";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}