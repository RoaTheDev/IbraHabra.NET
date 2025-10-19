namespace IbraHabra.NET.Application.Dto.Response;

public record RotateSecretResponse(string ClientId, string NewClientSecret);

public record AdminAppSummary(
    string Id,
    string ClientId,
    Guid ProjectId,
    string? DisplayName,
    string? ApplicationType,
    string? ClientType,
    string? ConsentType,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record ListApplicationsResponse(IEnumerable<AdminAppSummary> Items, string? NextCursor);