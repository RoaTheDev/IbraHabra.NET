namespace IbraHabra.NET.Domain.Contract.Services;

public interface ITokenService
{
    Task<string> GenerateAndStoreAsync(Guid userId);
    Task<bool> ValidateAndConsumeAsync(Guid userId, string refreshToken);
    Task RevokeAsync(Guid userId, string refreshToken);

    Task BlacklistAccessTokenAsync(Guid userId, string accessToken);
    Task<bool> IsAccessTokenBlacklistedAsync(Guid userId, string accessToken);

    void SetRefreshTokenCookie(HttpContext context, string refreshToken);
    void SetAccessTokenCookie(HttpContext context, string accessToken);
    void ClearRefreshTokenCookie(HttpContext context);
    void ClearAccessTokenCookie(HttpContext context);
}