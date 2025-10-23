namespace IbraHabra.NET.Domain.Contract;

public class RedisOptions
{
    public string? RedisConnectionString { get; set; }
    public bool UseRedis => !string.IsNullOrEmpty(RedisConnectionString);
}