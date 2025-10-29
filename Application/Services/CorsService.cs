using System.Text.RegularExpressions;
using IbraHabra.NET.Domain.Contract;
using IbraHabra.NET.Domain.Contract.Services;
using IbraHabra.NET.Domain.Entities;
using ImTools;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace IbraHabra.NET.Application.Services;

public class CorsService : ICorsService
{
    private readonly IRepo<Projects, Guid> _projectRepo;
    private readonly IRepo<OauthApplication, string> _clientRepo;
    private readonly ILogger<CorsService> _logger;
    private readonly ICacheService _cache;
    private const string CacheKeyPrefix = "cors_origins_";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromDays(7);

    public CorsService(IRepo<Projects, Guid> projectRepo, IRepo<OauthApplication, string> clientRepo,
        ILogger<CorsService> logger, ICacheService cache)
    {
        _projectRepo = projectRepo;
        _clientRepo = clientRepo;
        _logger = logger;
        _cache = cache;
    }

    public Task<bool> IsOriginAllowedAsync(string origin, Guid? projectId = null)
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsOriginAllowedForClientAsync(string origin, string clientId)
    {
        throw new NotImplementedException();
    }

    private List<string> ExtractUri(string? redirectUrisJson)
    {
        if (string.IsNullOrEmpty(redirectUrisJson))
            return new List<string>();

        var uris = JsonSerializer.Deserialize<string[]>(redirectUrisJson);
        if (uris is null) return new List<string>();

        var origins = new List<string>();
        foreach (var uri in uris)
        {
            if (Uri.TryCreate(uri, UriKind.Absolute, out var parsedUri))
            {
                var origin = $"{parsedUri.Scheme}://{parsedUri.Authority}";
                if (!origins.Contains(origin))
                    origins.Add(origin);
            }
        }

        return origins;
    }

    private bool IsOriginMatch(string origin, string pattern)
    {
        if (origin.Equals(pattern, StringComparison.OrdinalIgnoreCase)) return true;

        if (pattern.Contains('*'))
        {
            var regexPattern = "^" + Regex.Escape(pattern).Replace("\\*", ".*") + "$";
            return Regex.IsMatch(origin, regexPattern, RegexOptions.IgnoreCase);
        }

        return false;
    }
}