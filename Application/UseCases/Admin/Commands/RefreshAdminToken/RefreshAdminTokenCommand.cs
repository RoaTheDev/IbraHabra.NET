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

public record RefreshAdminTokenCommand;


public class RefreshAdminTokenHandler : IWolverineHandler
{
    public static async Task<ApiResult> Handle(
        RefreshAdminTokenCommand command,
        UserManager<User> userManager,
        IOptions<JwtOptions> jwtOptions,
        IHttpContextAccessor httpContextAccessor,
        ITokenService tokenService)
    {
        var context = httpContextAccessor.HttpContext!;
        if (!context.Request.Cookies.TryGetValue("refreshToken", out var refreshToken)
            || string.IsNullOrEmpty(refreshToken))
        {
            return ApiResult.Fail(
                ApiErrors.Authentication.InvalidToken());
        }

        if (!context.Request.Cookies.TryGetValue("accessToken", out var accessToken)
            || string.IsNullOrEmpty(accessToken))
        {
            return ApiResult.Fail(
                ApiErrors.Authentication.InvalidToken());
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var jwt = jwtOptions.Value;
        var key = Encoding.UTF8.GetBytes(jwt.Secret);

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

            var principal = tokenHandler.ValidateToken(accessToken, validationParameters, out _);

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return ApiResult.Fail(
                    ApiErrors.Authentication.InvalidToken());

            var isValid = await tokenService.ValidateAndConsumeAsync(userId, refreshToken);
            if (!isValid)
                return ApiResult.Fail(
                    ApiErrors.Authentication.InvalidToken());

            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return ApiResult.Fail(ApiErrors.User.NotFound());

            var roles = await userManager.GetRolesAsync(user);
            if (!roles.Contains("Admin") && !roles.Contains("Super"))
                return ApiResult.Fail(
                    ApiErrors.Authorization.InsufficientPermissions());

            await tokenService.BlacklistAccessTokenAsync(userId, accessToken);

            var newToken = await JwtGen.GenerateJwtToken(user, userManager, jwtOptions);

            var newRefreshToken = await tokenService.GenerateAndStoreAsync(user.Id);
            tokenService.SetAccessTokenCookie(context, newToken);
            tokenService.SetRefreshTokenCookie(context, newRefreshToken);

            return ApiResult.Ok();
        }
        catch (SecurityTokenException)
        {
            return ApiResult.Fail(
                ApiErrors.Authentication.InvalidToken());
        }
    }
}