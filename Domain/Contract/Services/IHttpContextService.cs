namespace IbraHabra.NET.Domain.Contract.Services;

public interface IHttpContextService
{
    string GetIpAddress();
    string GetUserAgent();
    string HashIpAddress(string ipAddress);
    DeviceInfoResult ParseDeviceInfo(string userAgent);
}

public record DeviceInfoResult(
    string DeviceName,
    string Browser,
    string Os,
    string FormattedString
);