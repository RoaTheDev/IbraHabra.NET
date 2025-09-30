namespace IbraHabra.NET.Domain.SharedKernel.Interface.Services;

public interface ITwoFactorTokenService
{
    Task<string> CreateTokenAsync(Guid userId, string clientId);
    Task<(Guid UserId, string ClientId)?> ValidateTokenAsync(string token);
    Task InvalidateTokenAsync(string token);
}
