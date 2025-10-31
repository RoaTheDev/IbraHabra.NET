using System.Text.Json;
using IbraHabra.NET.Domain.Contract;
using IbraHabra.NET.Domain.Contract.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace IbraHabra.NET.Application.Services;

public class CacheService : ICacheService
{
    private readonly IDistributedCache? _redisCache;
    private readonly IMemoryCache _memoryCache;

    private readonly bool _useRedis;

    public CacheService(
        IDistributedCache? redisCache,
        IMemoryCache memoryCache,
        IOptions<RedisOptions> options)
    {
        _redisCache = redisCache;
        _memoryCache = memoryCache;
        _useRedis = options.Value.UseRedis && redisCache != null;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, bool sliding = false)
    {
        var json = JsonSerializer.Serialize(value);

        if (_useRedis)
        {
            var cacheOptions = new DistributedCacheEntryOptions();

            if (sliding)
                cacheOptions.SlidingExpiration = expiration ?? TimeSpan.FromMinutes(15);
            else
                cacheOptions.AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(15);

            await _redisCache!.SetStringAsync(key, json, cacheOptions);
        }
        else
        {
            var memoryOptions = new MemoryCacheEntryOptions();
            if (sliding)
                memoryOptions.SlidingExpiration = expiration ?? TimeSpan.FromMinutes(15);
            else
                memoryOptions.AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(15);


            _memoryCache.Set(key, json, memoryOptions);
        }
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        string? json = null;
        if (_useRedis)
        {
            json = await _redisCache!.GetStringAsync(key);
        }
        else if (_memoryCache.TryGetValue(key, out string? memValue))
        {
            json = memValue;
        }

        return string.IsNullOrEmpty(json) ? default : JsonSerializer.Deserialize<T>(json);
    }

    public async Task RemoveAsync(string key)
    {
        if (_useRedis)
        {
            await _redisCache!.RemoveAsync(key);
        }

        _memoryCache.Remove(key);
    }
}