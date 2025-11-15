using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IbraHabra.NET.Application.Dto;
using IbraHabra.NET.Domain.Constants;
using IbraHabra.NET.Domain.Contract;
using IbraHabra.NET.Domain.Contract.Services;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Admin.Commands;

public record LogoutAdminCommand;

public class LogoutAdminHandler : IWolverineHandler
{
    public static async Task<ApiResult> Handle(
        LogoutAdminCommand command,
        IHttpContextAccessor httpContextAccessor,
        ITokenService tokenService,
        IOptions<JwtOptions> jwtOptions)
    {
        var context = httpContextAccessor.HttpContext!;

        var authHeader = context.Request.Headers["Authorization"].ToString();
        if (authHeader.StartsWith("Bearer "))
        {
            var token = authHeader.Substring("Bearer ".Length);
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var jwt = jwtOptions.Value;
                var key = Encoding.UTF8.GetBytes(jwt.Secret);
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

                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    await tokenService.BlacklistAccessTokenAsync(userId, token);

                    if (context.Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
                    {
                        await tokenService.RevokeAsync(userId, refreshToken);
                    }
                }
            }
            catch (SecurityTokenException)
            {
                return ApiResult.Fail(ApiErrors.Authentication.InvalidToken());
            }
        }

        tokenService.ClearRefreshTokenCookie(context);
        tokenService.ClearAccessTokenCookie(context);
        return ApiResult.Ok();
    }
}