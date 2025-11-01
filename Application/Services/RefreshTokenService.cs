using IbraHabra.NET.Domain.Contract.Services;

namespace IbraHabra.NET.Application.Services;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly ICacheService _cache;
    private const string CacheKeyPrefix = "refresh:";
    private const string CookieName = "refreshToken";
    private const int RefreshTokenExpirationDays = 7;

    public RefreshTokenService(ICacheService cache)
    {
        _cache = cache;
    }

    public async Task<string> GenerateAndStoreAsync(Guid userId)
    {
        var refreshToken = GenerateSecureToken();

        await _cache.SetAsync(
            $"{CacheKeyPrefix}{userId}:{refreshToken}",
            true,
            TimeSpan.FromDays(RefreshTokenExpirationDays));

        return refreshToken;
    }

    public async Task<bool> ValidateAndConsumeAsync(Guid userId, string refreshToken)
    {
        var cacheKey = $"{CacheKeyPrefix}{userId}:{refreshToken}";

        var exists = await _cache.GetAsync<bool>(cacheKey);

        if (!exists)
            return false;

        await _cache.RemoveAsync(cacheKey);

        return true;
    }

    public async Task RevokeAsync(Guid userId, string refreshToken)
    {
        await _cache.RemoveAsync($"{CacheKeyPrefix}{userId}:{refreshToken}");
    }

    public void SetRefreshTokenCookie(HttpContext context, string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(RefreshTokenExpirationDays),
            Path = "/api/v1/api/admin/auth/refresh"
        };

        context.Response.Cookies.Append(CookieName, refreshToken, cookieOptions);
    }

    public void ClearRefreshTokenCookie(HttpContext context)
    {
        context.Response.Cookies.Delete(CookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/api/v1/api/admin/auth/refresh"
        });
    }

    private static string GenerateSecureToken()
    {
        var randomBytes = new byte[64];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}