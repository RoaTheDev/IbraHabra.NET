using System.Security.Cryptography;
using System.Text;
using IbraHabra.NET.Domain.Contract.Services;

namespace IbraHabra.NET.Application.Services;

public class TokenService : ITokenService
{
    private readonly ICacheService _cache;
    private const string RefreshCacheKeyPrefix = "refresh:";
    private const string AccessCacheKeyPrefix = "access:";
    private const string RefreshCookieName = "refreshToken";
    private const string AccessCookieName = "accessToken";
    private const int RefreshTokenExpirationDays = 7;
    private const int AccessTokenExpirationHours = 8;

    public TokenService(ICacheService cache)
    {
        _cache = cache;
    }

    public async Task<string> GenerateAndStoreAsync(Guid userId)
    {
        var refreshToken = GenerateSecureToken();

        await _cache.SetAsync(
            $"{RefreshCacheKeyPrefix}{userId}:{refreshToken}",
            true,
            TimeSpan.FromDays(RefreshTokenExpirationDays));

        return refreshToken;
    }

    public async Task<bool> ValidateAndConsumeAsync(Guid userId, string refreshToken)
    {
        var cacheKey = $"{RefreshCacheKeyPrefix}{userId}:{refreshToken}";
        var exists = await _cache.GetAsync<bool>(cacheKey);

        if (!exists)
            return false;

        await _cache.RemoveAsync(cacheKey);
        return true;
    }

    public async Task RevokeAsync(Guid userId, string refreshToken)
    {
        await _cache.RemoveAsync($"{RefreshCacheKeyPrefix}{userId}:{refreshToken}");
    }

    public async Task StoreAccessTokenHashAsync(Guid userId, string accessToken)
    {
        var hash = HashToken(accessToken);
        await _cache.SetAsync(
            $"{AccessCacheKeyPrefix}{userId}:{hash}",
            true,
            TimeSpan.FromHours(AccessTokenExpirationHours));
    }

    public async Task<bool> ValidateAccessTokenHashAsync(Guid userId, string accessToken)
    {
        var hash = HashToken(accessToken);
        var cacheKey = $"{AccessCacheKeyPrefix}{userId}:{hash}";
        return await _cache.GetAsync<bool>(cacheKey);
    }

    public void SetRefreshTokenCookie(HttpContext context, string refreshToken)
    {
        var apiVersion = context.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(RefreshTokenExpirationDays),
            Path = $"/v{apiVersion}/api/admin/auth/refresh"
        };

        context.Response.Cookies.Append(RefreshCookieName, refreshToken, cookieOptions);
    }

    public void SetAccessTokenCookie(HttpContext context, string accessToken)
    {
        var apiVersion = context.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddHours(AccessTokenExpirationHours),
            Path = $"/v{apiVersion}/api/"
        };

        context.Response.Cookies.Append(AccessCookieName, accessToken, cookieOptions);
    }

    public void ClearRefreshTokenCookie(HttpContext context)
    {
        var apiVersion = context.GetRequestedApiVersion()?.ToString() ?? "1.0";
        context.Response.Cookies.Delete(RefreshCookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = $"/v{apiVersion}/api/admin/auth/refresh"
        });
    }

    public void ClearAccessTokenCookie(HttpContext context)
    {
        var apiVersion = context.GetRequestedApiVersion()?.ToString() ?? "1.0";
        context.Response.Cookies.Delete(AccessCookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = $"/v{apiVersion}/api/"
        });
    }

    private static string GenerateSecureToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private static string HashToken(string token)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(hashBytes);
    }
}

