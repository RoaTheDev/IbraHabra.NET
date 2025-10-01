namespace IbraHabra.NET.Domain.SharedKernel.Interface.Services;

public interface ITwoFactorTokenService
{
    Task<string> CreateTokenAsync(Guid userId, string clientId);
  
    Task<(Guid UserId, string ClientId)?> ValidateAndRemoveTokenAsync(string token);
}
