using System.Text.Json;
using IbraHabra.NET.Domain.Contract;
using IbraHabra.NET.Domain.Contract.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace IbraHabra.NET.Application.Services;

/// <summary>
/// Cache service with automatic fallback from Redis to in-memory cache.
/// If Redis is down, operations seamlessly fall back to memory cache.
/// </summary>
public class CacheService : ICacheService
{
    private readonly IDistributedCache? _redisCache;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<CacheService> _logger;
    private readonly bool _useRedis;

    public CacheService(
        IDistributedCache? redisCache,
        IMemoryCache memoryCache,
        ILogger<CacheService> logger,
        IOptions<RedisOptions> options)
    {
        _redisCache = redisCache;
        _memoryCache = memoryCache;
        _logger = logger;
        _useRedis = options.Value.UseRedis && redisCache != null;
    }

   public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, bool sliding = false)
{
    var serialized = JsonSerializer.Serialize(value);
    
    var cacheOptions = new MemoryCacheEntryOptions();
    if (expiration.HasValue)
    {
        if (sliding)
            cacheOptions.SetSlidingExpiration(expiration.Value);
        else
            cacheOptions.SetAbsoluteExpiration(expiration.Value);
    }

    _memoryCache.Set(key, serialized, cacheOptions);

    if (_redisCache != null)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                var options = new DistributedCacheEntryOptions();

                if (expiration.HasValue)
                {
                    if (sliding)
                        options.SetSlidingExpiration(expiration.Value);
                    else
                        options.SetAbsoluteExpiration(expiration.Value);
                }

                using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));
                await _redisCache.SetStringAsync(key, serialized, options, cts.Token);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis SET failed for key '{Key}' (background)", key);
            }
        });
    }

    return Task.CompletedTask;
}

public async Task<T?> GetAsync<T>(string key)
{
    string? json = null;

    if (_useRedis)
    {
        try
        {
            json = await _redisCache!.GetStringAsync(key);

            if (!string.IsNullOrEmpty(json))
            {
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(15));
                _memoryCache.Set(key, json, cacheOptions);
                
                return JsonSerializer.Deserialize<T>(json);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Redis GET failed for key '{Key}', falling back to in-memory cache", key);
        }
    }

    if (_memoryCache.TryGetValue(key, out string? memValue))
    {
        json = memValue;
    }

    return string.IsNullOrEmpty(json) ? default : JsonSerializer.Deserialize<T>(json);
}

    public async Task RemoveAsync(string key)
    {
        if (_useRedis)
        {
            try
            {
                await _redisCache!.RemoveAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Redis REMOVE failed for key '{Key}'", key);
            }
        }

        _memoryCache.Remove(key);
    }

}