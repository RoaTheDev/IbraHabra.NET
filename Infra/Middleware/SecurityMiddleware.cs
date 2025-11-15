namespace IbraHabra.NET.Infra.Middleware;

public class SecurityMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SecurityMiddleware> _logger;

    public SecurityMiddleware(
        RequestDelegate next,
        IConfiguration configuration,
        ILogger<SecurityMiddleware> logger)
    {
        _next = next;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        AddSecurityHeaders(context);

        if (IsStateMutatingRequest(context.Request))
        {
            if (!ValidateCsrfProtection(context))
            {
                _logger.LogWarning(
                    "CSRF validation failed for {Method} {Path} from {IP}",
                    context.Request.Method,
                    context.Request.Path,
                    context.Connection.RemoteIpAddress);

                context.Response.StatusCode = 403;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "CSRF validation failed",
                    message = "Request must include X-Requested-With header"
                });
                return;
            }
        }

        await _next(context);
    }

    private void AddSecurityHeaders(HttpContext context)
    {
        var headers = context.Response.Headers;
        var isDevelopment = _configuration.GetValue<bool>("IsDevelopment");

        // Content Security Policy 
        var csp = "default-src 'self'; " +
                  "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " + // unsafe-eval for development tools
                  "style-src 'self' 'unsafe-inline'; " + // unsafe-inline needed for Tailwind/CSS-in-JS
                  "img-src 'self' data: https:; " + // Allow images from HTTPS and data URLs
                  "font-src 'self' data:; " +
                  "connect-src 'self' " + _configuration["ApiUrl"] + "; " + // API calls
                  "frame-ancestors 'none'; " + // Prevent clickjacking
                  "base-uri 'self'; " +
                  "form-action 'self'";

        if (!isDevelopment)
        {
            csp += "; upgrade-insecure-requests";
        }

        headers.Append("Content-Security-Policy", csp);

        headers.Append("X-Content-Type-Options", "nosniff");

        headers.Append("X-Frame-Options", "DENY");

        headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

        headers.Append("Permissions-Policy",
            "geolocation=(), " +
            "microphone=(), " +
            "camera=(), " +
            "payment=(), " +
            "usb=(), " +
            "magnetometer=(), " +
            "gyroscope=(), " +
            "accelerometer=()");

        if (!isDevelopment)
        {
            headers.Append("Strict-Transport-Security",
                "max-age=31536000; includeSubDomains; preload");
        }

        headers.Remove("Server");
        headers.Remove("X-Powered-By");
        headers.Remove("X-AspNet-Version");
    }

    private bool IsStateMutatingRequest(HttpRequest request)
    {
        return request.Method is "POST" or "PUT" or "PATCH" or "DELETE";
    }

    private bool ValidateCsrfProtection(HttpContext context)
    {
        var request = context.Request;


        var path = request.Path.Value?.ToLower() ?? string.Empty;

        if (path.Contains("/webhooks/"))
        {
            return true;
        }

        if (!request.Headers.TryGetValue("X-Requested-With", out var headerValue) ||
            headerValue != "XMLHttpRequest")
        {
            return false;
        }

        var allowedOrigins = _configuration.GetSection("AllowedOrigins").Get<string[]>()
                             ?? new[] { _configuration["ClientUrl"] ?? "http://localhost:3000" };

        if (request.Headers.TryGetValue("Origin", out var origin))
        {
            if (!allowedOrigins.Any(allowed =>
                    origin.ToString().StartsWith(allowed, StringComparison.OrdinalIgnoreCase)))
            {
                _logger.LogWarning("Invalid origin: {Origin}", origin!);
                return false;
            }
        }

        return true;
    }
}