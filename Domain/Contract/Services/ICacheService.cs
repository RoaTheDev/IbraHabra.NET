namespace IbraHabra.NET.Domain.Contract.Services;

public interface ICacheService
{
    Task SetAsync(string key, string value, bool sliding = false, TimeSpan? expiration = null);
    Task<string?> GetAsync(string key);
    Task RemoveAsync(string key);
}