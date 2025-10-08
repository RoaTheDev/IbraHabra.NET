using IbraHabra.NET.Domain.Contract.Services;

namespace IbraHabra.NET.Application.Services;


public class GeoLocationService : IGeoLocationService
{   
    private readonly HttpClient _httpClient;
    private readonly ILogger<GeoLocationService> _logger;

    public GeoLocationService(HttpClient httpClient, ILogger<GeoLocationService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<GeoLocationResult> GetLocationAsync(
        string ipAddress, 
        CancellationToken cancellationToken = default)
    {
        // Handle local/private IPs
        if (IsPrivateOrLocalIp(ipAddress))
        {
            return new GeoLocationResult("Unknown", "Unknown", "Unknown", null, null);
        }

        try
        {
            // Using ip-api.com (free, no key required, 45 req/min limit)
            var response = await _httpClient.GetFromJsonAsync<IpApiResponse>(
                $"http://ip-api.com/json/{ipAddress}?fields=status,message,country,regionName,city,lat,lon",
                cancellationToken);

            if (response?.Status == "success")
            {
                return new GeoLocationResult(
                    response.Country ?? "Unknown",
                    response.City ?? "Unknown",
                    response.RegionName ?? "Unknown",
                    response.Lat,
                    response.Lon
                );
            }

            _logger.LogWarning("GeoLocation API failed for IP {IpAddress}: {Message}", 
                ipAddress, response?.Message ?? "Unknown error");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting geolocation for IP {IpAddress}", ipAddress);
        }

        return new GeoLocationResult("Unknown", "Unknown", "Unknown", null, null);
    }

    private static bool IsPrivateOrLocalIp(string ipAddress)
    {
        if (string.IsNullOrEmpty(ipAddress) || ipAddress == "Unknown")
            return true;

        // Localhost
        if (ipAddress == "::1" || ipAddress == "127.0.0.1" || ipAddress.StartsWith("localhost"))
            return true;

        // Private IP ranges
        var parts = ipAddress.Split('.');
        if (parts.Length != 4) return true; // IPv6 or invalid

        if (!int.TryParse(parts[0], out int firstOctet))
            return true;

        // 10.0.0.0 - 10.255.255.255
        if (firstOctet == 10) return true;

        // 172.16.0.0 - 172.31.255.255
        if (firstOctet == 172 && int.TryParse(parts[1], out int secondOctet) && secondOctet is >= 16 and <= 31)
            return true;

        // 192.168.0.0 - 192.168.255.255
        if (firstOctet == 192 && parts[1] == "168")
            return true;

        return false;
    }

    private class IpApiResponse
    {
        public string? Status { get; set; }
        public string? Message { get; set; }
        public string? Country { get; set; }
        public string? RegionName { get; set; }
        public string? City { get; set; }
        public double? Lat { get; set; }
        public double? Lon { get; set; }
    }
}