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

    public async Task SetAsync(string key, string value, bool sliding = false, TimeSpan? expiration = null)
    {
        var cacheOptions = new DistributedCacheEntryOptions();
        if (expiration.HasValue)
        {
            if (sliding)
                cacheOptions.SlidingExpiration = expiration;
            else
                cacheOptions.AbsoluteExpirationRelativeToNow = expiration;
        }

        if (_useRedis)
        {
            await _redisCache!.SetStringAsync(key, value, cacheOptions);
        }
        else
        {
            var memoryOptions = new MemoryCacheEntryOptions();
            if (expiration.HasValue)
            {
                if (sliding)
                    memoryOptions.SlidingExpiration = expiration;
                else
                    memoryOptions.AbsoluteExpirationRelativeToNow = expiration;
            }

            _memoryCache.Set(key, value, memoryOptions);
        }
    }

    public async Task<string?> GetAsync(string key)
    {
        if (_useRedis)
        {
            var value = await _redisCache!.GetStringAsync(key);
            if (!string.IsNullOrEmpty(value)) return value;
        }

        return _memoryCache.TryGetValue(key, out string? memValue) ? memValue : null;
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