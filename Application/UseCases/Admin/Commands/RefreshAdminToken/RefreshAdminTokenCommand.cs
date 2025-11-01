using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IbraHabra.NET.Application.Dto;
using IbraHabra.NET.Application.Utils;
using IbraHabra.NET.Domain.Constants;
using IbraHabra.NET.Domain.Contract;
using IbraHabra.NET.Domain.Contract.Services;
using IbraHabra.NET.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
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
        IOptions<JwtOptions> jwtOptions,
        IHttpContextAccessor httpContextAccessor,
        IRefreshTokenService refreshTokenService)
    {
        var context = httpContextAccessor.HttpContext!;

        if (!context.Request.Cookies.TryGetValue("refreshToken", out var refreshToken)
            || string.IsNullOrEmpty(refreshToken))
        {
            return ApiResult<RefreshAdminTokenResponse>.Fail(ApiErrors.Authentication.InvalidToken());
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var jwt = jwtOptions.Value;
        var jwtSecret = jwt.Secret;


        var key = Encoding.UTF8.GetBytes(jwtSecret);

        try
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = jwt.Issuer,
                ValidateAudience = true,
                ValidAudience = jwt.Audience,
                ValidateLifetime = false,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(command.Token, validationParameters, out _);

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return ApiResult<RefreshAdminTokenResponse>.Fail(ApiErrors.Authentication.InvalidToken());

            var isValid = await refreshTokenService.ValidateAndConsumeAsync(userId, refreshToken);
            if (!isValid)
                return ApiResult<RefreshAdminTokenResponse>.Fail(ApiErrors.Authentication.InvalidToken());

            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return ApiResult<RefreshAdminTokenResponse>.Fail(ApiErrors.User.NotFound());

            var roles = await userManager.GetRolesAsync(user);
            if (!roles.Contains("Admin") && !roles.Contains("Super"))
                return ApiResult<RefreshAdminTokenResponse>.Fail(ApiErrors.Authorization.InsufficientPermissions());

            var newToken = await JwtGen.GenerateJwtToken(user, userManager, jwtOptions);
            var expiresAt = DateTime.UtcNow.AddHours(8);

            var newRefreshToken = await refreshTokenService.GenerateAndStoreAsync(user.Id);
            refreshTokenService.SetRefreshTokenCookie(context, newRefreshToken);

            return ApiResult<RefreshAdminTokenResponse>.Ok(new RefreshAdminTokenResponse(
                Token: newToken,
                ExpiresAt: expiresAt));
        }
        catch (SecurityTokenException)
        {
            return ApiResult<RefreshAdminTokenResponse>.Fail(ApiErrors.Authentication.InvalidToken());
        }
    }
}