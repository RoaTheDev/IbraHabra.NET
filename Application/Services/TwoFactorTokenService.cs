using IbraHabra.NET.Domain.SharedKernel.Interface.Services;
using Microsoft.Extensions.Caching.Memory;

namespace IbraHabra.NET.Application.Services;

public class TwoFactorTokenService : ITwoFactorTokenService
{
    private readonly IMemoryCache _cache;

    public TwoFactorTokenService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<string> CreateTokenAsync(Guid userId, string clientId)
    {
        var token = Guid.CreateVersion7().ToString();
        var data = new { UserId = userId, ClientId = clientId };

        _cache.Set($"2fa:{token}", data, TimeSpan.FromMinutes(5));

        return Task.FromResult(token);
    }

    public Task<(Guid UserId, string ClientId)?> ValidateTokenAsync(string token)
    {
        if (_cache.TryGetValue($"2fa:{token}", out var data))
        {
            dynamic tokenData = data!;
            return Task.FromResult<(Guid, string)?>(((Guid)tokenData.UserId, (string)tokenData.ClientId));
        }

        return Task.FromResult<(Guid, string)?>(null);
    }

    public Task InvalidateTokenAsync(string token)
    {
        _cache.Remove($"2fa:{token}");
        return Task.CompletedTask;
    }
}