namespace IbraHabra.NET.Domain.Contract.Services;

public interface IGeoLocationService
{
    Task<GeoLocationResult> GetLocationAsync(string ipAddress, CancellationToken cancellationToken = default);
}

public record GeoLocationResult(
    string Country,
    string City,
    string Region,
    double? Latitude,
    double? Longitude
);