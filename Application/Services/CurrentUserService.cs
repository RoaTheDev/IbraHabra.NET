using System.Security.Claims;
using IbraHabra.NET.Domain.SharedKernel.Interface;
using IbraHabra.NET.Domain.SharedKernel.Interface.Services;

namespace IbraHabra.NET.Application.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContext)
    {
        _httpContextAccessor = httpContext;
    }

    public Guid UserId
    {
        get
        {
            if (!IsAuthenticated)
                throw new UnauthorizedAccessException("User not authenticated");

            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("sub")
                              ?? _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)
                              ?? _httpContextAccessor.HttpContext?.User?.FindFirst(
                                  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");

            if (userIdClaim is not null && Guid.TryParse(userIdClaim.Value, out var userId))
                return userId;

            throw new UnauthorizedAccessException("User ID not found or invalid format");
        }
    }

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public string? Email
    {
        get
        {
            if (!IsAuthenticated)
                return null;

            return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value
                   ?? _httpContextAccessor.HttpContext?.User?.FindFirst("email")?.Value;
        }
    }
}