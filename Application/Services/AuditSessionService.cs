using IbraHabra.NET.Domain.Contract;
using IbraHabra.NET.Domain.Contract.Services;
using IbraHabra.NET.Domain.Entities;
using IbraHabra.NET.Infra.Persistent;

namespace IbraHabra.NET.Application.Services;

public class AuditAndSessionService : IAuditAndSessionService
{
    private readonly IRepo<UserAuditTrail, Guid> _auditRepo;
    private readonly IRepo<UserSession, Guid> _sessionRepo;
    private readonly IHttpContextService _httpContextService;
    private readonly IGeoLocationService _geoLocationService;
    private readonly AppDbContext _context; 

    public AuditAndSessionService(
        IRepo<UserAuditTrail, Guid> auditRepo,
        IRepo<UserSession, Guid> sessionRepo,
        IHttpContextService httpContextService,
        IGeoLocationService geoLocationService,
        AppDbContext context)
    {
        _auditRepo = auditRepo;
        _sessionRepo = sessionRepo;
        _httpContextService = httpContextService;
        _geoLocationService = geoLocationService;
        _context = context;
    }

    public async Task<UserAuditTrail> CreateAuditTrailAsync(
        Guid userId,
        string? clientId,
        bool isSuccessful,
        string? failureReason = null,
        CancellationToken cancellationToken = default)
    {
        var ipAddress = _httpContextService.GetIpAddress();
        var userAgent = _httpContextService.GetUserAgent();
        var geoLocation = await _geoLocationService.GetLocationAsync(ipAddress, cancellationToken);

        // Check for security anomalies
        var hasAnomaly = await CheckForSecurityAnomaliesAsync(
            userId, 
            geoLocation.Country, 
            geoLocation.City, 
            cancellationToken);

        string? alertType = null;
        if (hasAnomaly)
        {
            alertType = await DetermineAlertTypeAsync(userId, geoLocation, cancellationToken);
        }

        var auditTrail = new UserAuditTrail
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            IpAddress = ipAddress,
            IpAddressHash = _httpContextService.HashIpAddress(ipAddress),
            UserAgent = userAgent,
            Country = geoLocation.Country,
            City = geoLocation.City,
            Region = geoLocation.Region,
            Latitude = geoLocation.Latitude,
            Longitude = geoLocation.Longitude,
            ClientId = clientId,
            IsSuccessful = isSuccessful,
            FailureReason = failureReason,
            IsAlertTriggered = hasAnomaly,
            AlertType = alertType,
            LoginAt = DateTime.UtcNow
        };

        await _auditRepo.AddAsync(auditTrail);
        await _context.SaveChangesAsync(cancellationToken);
        
        return auditTrail;
    }

    public async Task<UserSession> CreateSessionAsync(
        Guid userId,
        string clientId,
        CancellationToken cancellationToken = default)
    {
        var ipAddress = _httpContextService.GetIpAddress();
        var userAgent = _httpContextService.GetUserAgent();
        var deviceInfo = _httpContextService.ParseDeviceInfo(userAgent);
        var geoLocation = await _geoLocationService.GetLocationAsync(ipAddress, cancellationToken);

        var session = new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SessionToken = Guid.NewGuid().ToString("N"), 
            ClientId = clientId,
            DeviceInfo = deviceInfo.FormattedString,
            IpAddress = ipAddress,
            Country = geoLocation.Country,
            CreatedAt = DateTime.UtcNow,
            LastActiveAt = DateTime.UtcNow,
            IsTrusted = false,
            IsCurrent = true
        };

        await _sessionRepo.AddAsync(session);
        await _context.SaveChangesAsync(cancellationToken);
        
        return session;
    }

    public async Task MarkOtherSessionsAsNotCurrentAsync(
        Guid userId,
        Guid currentSessionId,
        CancellationToken cancellationToken = default)
    {
        await _sessionRepo.BatchUpdateAsync(
            s => s.UserId == userId && s.Id != currentSessionId && s.IsCurrent,
            s => s.SetProperty(x => x.IsCurrent, false));
        
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> CheckForSecurityAnomaliesAsync(
        Guid userId,
        string country,
        string city,
        CancellationToken cancellationToken = default)
    {
        var yesterday = DateTime.UtcNow.AddHours(-24);
        var recentLogins = await _auditRepo.GetAllViaConditionAsync(
            a => a.UserId == userId 
                && a.IsSuccessful 
                && a.LoginAt >= yesterday);

        var userAuditTrails = recentLogins.ToList();
        if (!userAuditTrails.Any())
            return false;

        var lastLogin = userAuditTrails.OrderByDescending(a => a.LoginAt).First();

        // Check for impossible travel (different country within short time)
        if (lastLogin.Country != country && lastLogin.Country != "Unknown" && country != "Unknown")
        {
            var timeDiff = DateTime.UtcNow - lastLogin.LoginAt;
            if (timeDiff.TotalHours < 2) // Less than 2 hours between different countries
                return true;
        }

        // Check for brute force (multiple failed attempts recently)
        var recentFailed = await _auditRepo.GetAllViaConditionAsync(
            a => a.UserId == userId 
                && !a.IsSuccessful 
                && a.LoginAt >= DateTime.UtcNow.AddMinutes(-30));

        if (recentFailed.Count() >= 5)
            return true;

        return false;
    }

    private async Task<string> DetermineAlertTypeAsync(
        Guid userId,
        GeoLocationResult geoLocation,
        CancellationToken cancellationToken)
    {
        var yesterday = DateTime.UtcNow.AddHours(-24);
        var recentLogins = await _auditRepo.GetAllViaConditionAsync(
            a => a.UserId == userId 
                && a.IsSuccessful 
                && a.LoginAt >= yesterday);

        var userAuditTrails = recentLogins.ToList();
        if (userAuditTrails.Any())
        {
            var lastLogin = userAuditTrails.OrderByDescending(a => a.LoginAt).First();
            if (lastLogin.Country != geoLocation.Country && lastLogin.Country != "Unknown" && geoLocation.Country != "Unknown")
            {
                var timeDiff = DateTime.UtcNow - lastLogin.LoginAt;
                if (timeDiff.TotalHours < 2)
                    return "ImpossibleTravel";
            }
        }

        var recentFailed = await _auditRepo.GetAllViaConditionAsync(
            a => a.UserId == userId 
                && !a.IsSuccessful 
                && a.LoginAt >= DateTime.UtcNow.AddMinutes(-30));

        if (recentFailed.Count() >= 5)
            return "BruteForce";

        return "NewLocation";
    }
}