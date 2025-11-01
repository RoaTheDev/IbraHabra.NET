namespace IbraHabra.NET.Domain.Contract.Services;

public interface IRefreshTokenService
{
    Task<string> GenerateAndStoreAsync(Guid userId);
    Task<bool> ValidateAndConsumeAsync(Guid userId, string refreshToken);
    Task RevokeAsync(Guid userId, string refreshToken);
    void SetRefreshTokenCookie(HttpContext context, string refreshToken);
    void ClearRefreshTokenCookie(HttpContext context);
}