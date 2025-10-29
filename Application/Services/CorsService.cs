// using System.Text.RegularExpressions;
// using IbraHabra.NET.Domain.Contract;
// using IbraHabra.NET.Domain.Contract.Services;
// using IbraHabra.NET.Domain.Entities;
// using ImTools;
// using Microsoft.CodeAnalysis;
// using Newtonsoft.Json;
// using JsonSerializer = System.Text.Json.JsonSerializer;
//
// namespace IbraHabra.NET.Application.Services;
//
// public class CorsService : ICorsService
// {
//     private readonly IRepo<Projects, Guid> _projectRepo;
//     private readonly IRepo<OauthApplication, string> _clientRepo;
//     private readonly ILogger<CorsService> _logger;
//     private readonly ICacheService _cache;
//     private const string CacheKeyPrefix = "cors_origins_";
//     private static readonly TimeSpan CacheDuration = TimeSpan.FromDays(7);
//
//     public CorsService(IRepo<Projects, Guid> projectRepo, IRepo<OauthApplication, string> clientRepo,
//         ILogger<CorsService> logger, ICacheService cache)
//     {
//         _projectRepo = projectRepo;
//         _clientRepo = clientRepo;
//         _logger = logger;
//         _cache = cache;
//     }
//
//     public Task<bool> IsOriginAllowedAsync(string origin, Guid? projectId = null)
//     {
//         throw new NotImplementedException();
//     }
//
//     public async Task<bool> IsOriginAllowedForClientAsync(string origin, string clientId)
//     {
//         if (string.IsNullOrEmpty(origin) || string.IsNullOrEmpty(clientId))
//             return false;
//
//         var cacheKey = $"{CacheKeyPrefix}client_{origin}_{clientId}";
//
//         var cached = await _cache.GetAsync<bool?>(cacheKey);
//         if (cached.HasValue)
//             return cached.Value;
//
//         throw new NotImplementedException();
//     }
//
//     private async Task<List<string>> GetAllAllowedPatternAsync(Guid projectId)
//     {
//         var cacheKey = $"{CacheKeyPrefix}patterns_{projectId}";
//         var res = await _cache.GetAsync<List<string>>(cacheKey);
//         if (res is not null)
//             return res;
//         var allowedOrigins = await _projectRepo.GetViaConditionAsync(p =>
//                 p.Id == projectId && p.IsActive,
//             projection => projection.AllowedCorsOrigins
//         );
//         if (allowedOrigins is null)
//             return [];
//
//         var allPatterns = new List<string>();
//         var patterns = ParseCorsOrigins(allowedOrigins);
//         allPatterns.AddRange(patterns);
//
//         await _cache.SetAsync(cacheKey, allPatterns, CacheDuration);
//         return allPatterns;
//     }
//
//     private List<string> ExtractUri(string? redirectUrisJson)
//     {
//         if (string.IsNullOrEmpty(redirectUrisJson))
//             return new List<string>();
//
//         var uris = JsonSerializer.Deserialize<string[]>(redirectUrisJson);
//         if (uris is null) return new List<string>();
//
//         var origins = new List<string>();
//         foreach (var uri in uris)
//         {
//             if (Uri.TryCreate(uri, UriKind.Absolute, out var parsedUri))
//             {
//                 var origin = $"{parsedUri.Scheme}://{parsedUri.Authority}";
//                 if (!origins.Contains(origin))
//                     origins.Add(origin);
//             }
//         }
//
//         return origins;
//     }
//
//     private List<string> ParseCorsOrigins(string? corsOriginsJson)
//     {
//         if (string.IsNullOrEmpty(corsOriginsJson))
//             return new List<string>();
//
//         try
//         {
//             var origins = JsonSerializer.Deserialize<string[]>(corsOriginsJson);
//             return origins?.ToList() ?? new List<string>();
//         }
//         catch (JsonException ex)
//         {
//             _logger.LogError(ex, "Failed to parse CORS origins JSON: {Json}", corsOriginsJson);
//             return new List<string>();
//         }
//     }
//
//     private bool IsOriginMatch(string origin, string pattern)
//     {
//         if (origin.Equals(pattern, StringComparison.OrdinalIgnoreCase)) return true;
//
//         if (pattern.Contains('*'))
//         {
//             var regexPattern = "^" + Regex.Escape(pattern).Replace("\\*", ".*") + "$";
//             return Regex.IsMatch(origin, regexPattern, RegexOptions.IgnoreCase);
//         }
//
//         return false;
//     }
// }