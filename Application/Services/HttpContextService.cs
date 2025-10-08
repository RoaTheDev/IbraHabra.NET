using System.Net;
using System.Security.Cryptography;
using System.Text;
using IbraHabra.NET.Domain.Contract.Services;
using UAParser;

namespace IbraHabra.NET.Application.Services;

public class HttpContextService : IHttpContextService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetIpAddress()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return "Unknown";

        // Check for forwarded IP (if behind proxy/load balancer)
        var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            var ips = forwardedFor.Split(',');
            return ips[0].Trim();
        }

        // Check X-Real-IP header
        var realIp = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
            return realIp;

        // Fallback to RemoteIpAddress
        return httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    public string GetUserAgent()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return "Unknown";

        return httpContext.Request.Headers["User-Agent"].ToString() ?? "Unknown";
    }

    public string HashIpAddress(string ipAddress)
    {
        if (string.IsNullOrEmpty(ipAddress)) return string.Empty;

        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(ipAddress);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    public DeviceInfoResult ParseDeviceInfo(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent))
            return new DeviceInfoResult("Unknown", "Unknown", "Unknown", "Unknown Device");

        try
        {
            var parser = Parser.GetDefault();
            var clientInfo = parser.Parse(userAgent);

            var deviceName = clientInfo.Device.Family != "Other"
                ? clientInfo.Device.Family
                : "Desktop";

            var browser = $"{clientInfo.UA.Family} {clientInfo.UA.Major}";
            var os = $"{clientInfo.OS.Family} {clientInfo.OS.Major}";

            var formatted = $"{browser} on {os}";
            if (deviceName != "Desktop" && deviceName != "Other")
                formatted = $"{deviceName} - {formatted}";

            return new DeviceInfoResult(deviceName, browser, os, formatted);
        }
        catch
        {
            return new DeviceInfoResult("Unknown", "Unknown", "Unknown", "Unknown Device");
        }
    }
}