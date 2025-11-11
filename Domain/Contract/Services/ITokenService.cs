namespace IbraHabra.NET.Domain.Contract.Services;

public interface ITokenService
{
    Task<string> GenerateAndStoreAsync(Guid userId);
    Task<bool> ValidateAndConsumeAsync(Guid userId, string refreshToken);
    Task RevokeAsync(Guid userId, string refreshToken);
    void SetRefreshTokenCookie(HttpContext context, string refreshToken);
    void ClearRefreshTokenCookie(HttpContext context);
    Task StoreAccessTokenHashAsync(Guid userId, string accessToken);
    void SetAccessTokenCookie(HttpContext context, string accessToken);
    void ClearAccessTokenCookie(HttpContext context);
    Task<bool> ValidateAccessTokenHashAsync(Guid userId, string accessToken);
}