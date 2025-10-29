using IbraHabra.NET.Domain.Contract;
using IbraHabra.NET.Domain.Contract.Services;
using IbraHabra.NET.Domain.Entities;
using Microsoft.AspNetCore.Cors.Infrastructure;

namespace IbraHabra.NET.Infra.Filters;

public class DynamicCorsPolicyProvider : ICorsPolicyProvider
{
    private readonly IRepo<ClientOrigin, Guid> _clientOrigins;
    private readonly ICacheService _cache;
    private readonly ILogger<DynamicCorsPolicyProvider> _logger;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public DynamicCorsPolicyProvider(
        IRepo<ClientOrigin, Guid> clientOrigins,
        ICacheService cache,
        ILogger<DynamicCorsPolicyProvider> logger)
    {
        _clientOrigins = clientOrigins;
        _cache = cache;
        _logger = logger;
    }

    public async Task<CorsPolicy?> GetPolicyAsync(HttpContext context, string? policyName)
    {
        var origin = context.Request.Headers.Origin.ToString();
        if (string.IsNullOrWhiteSpace(origin))
            return null;

        var clientId = context.Request.Query["client_id"].FirstOrDefault()
                       ?? context.Request.Headers["Client-Id"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(clientId))
            return null;

        if (!Guid.TryParse(clientId, out var clientGuid))
        {
            _logger.LogWarning("Invalid client_id format: {ClientId}", clientId);
            return null;
        }

        var cacheKey = $"cors:{clientGuid}";
        var allowedOrigins = await _cache.GetAsync<HashSet<string>>(cacheKey);

        if (allowedOrigins == null)
        {
            var origins = await _clientOrigins.GetAllViaConditionAsync(
                c => c.ClientId == clientGuid && c.IsActive,
                c => c.Origin
            );

            allowedOrigins = origins.ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (allowedOrigins.Count == 0)
            {
                _logger.LogWarning("No active origins found for client_id: {ClientId}", clientGuid);
                return null;
            }

            await _cache.SetAsync(cacheKey, allowedOrigins, CacheDuration);
        }

        if (!allowedOrigins.Contains(origin))
        {
            _logger.LogWarning(
                "Origin {Origin} not allowed for client_id: {ClientId}",
                origin,
                clientGuid
            );
            return null;
        }

        return new CorsPolicyBuilder()
            .WithOrigins(origin)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .WithExposedHeaders("WWW-Authenticate", "Content-Disposition")
            .Build();
    }
}