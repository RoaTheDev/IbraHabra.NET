namespace IbraHabra.NET.Domain.Contract.Services;

public interface ICacheService
{
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, bool sliding = false);
    Task<T?> GetAsync<T>(string key);
    Task RemoveAsync(string key);
}