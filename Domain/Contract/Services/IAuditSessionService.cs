using IbraHabra.NET.Domain.Entities;

namespace IbraHabra.NET.Domain.Contract.Services;

public interface IAuditAndSessionService
{
    Task<UserAuditTrail> CreateAuditTrailAsync(
        Guid userId,
        string? clientId,
        bool isSuccessful,
        string? failureReason = null,
        CancellationToken cancellationToken = default);

    Task<UserSession> CreateSessionAsync(
        Guid userId,
        string clientId,
        CancellationToken cancellationToken = default);

    Task MarkOtherSessionsAsNotCurrentAsync(
        Guid userId,
        Guid currentSessionId,
        CancellationToken cancellationToken = default);

    Task<bool> CheckForSecurityAnomaliesAsync(
        Guid userId,
        string country,
        string city,
        CancellationToken cancellationToken = default);
}