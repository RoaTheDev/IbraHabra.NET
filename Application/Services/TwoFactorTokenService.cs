using IbraHabra.NET.Domain.Contract.Services;
using Microsoft.Extensions.Caching.Memory;

namespace IbraHabra.NET.Application.Services;

public class TwoFactorTokenService : ITwoFactorTokenService
{
    private readonly IMemoryCache _cache;

    public TwoFactorTokenService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<(Guid UserId, string ClientId)?> ValidateAndRemoveTokenAsync(string token)
    {
        var key = $"2fa:{token}";

        if (_cache.TryGetValue(key, out var data))
        {
            // Remove immediately to ensure one-time use
            _cache.Remove(key);

            dynamic tokenData = data!;
            return Task.FromResult<(Guid, string)?>(((Guid)tokenData.UserId, (string)tokenData.ClientId));
        }

        return Task.FromResult<(Guid, string)?>(null);
    }

    public Task<string> CreateTokenAsync(Guid userId, string clientId)
    {
        var token = Guid.CreateVersion7().ToString();
        var data = new { UserId = userId, ClientId = clientId };

        _cache.Set($"2fa:{token}", data, TimeSpan.FromMinutes(5));

        return Task.FromResult(token);
    }
}