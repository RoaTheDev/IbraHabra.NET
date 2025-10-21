namespace IbraHabra.NET.Application.Dto.Response;


public record ClientSummary(
    string ClientId,
    Guid ProjectId,
    string? DisplayName,
    string? ApplicationType,
    string? ClientType,
    string? ConsentType,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record ListApplicationsResponse(IEnumerable<ClientSummary> Items, string? NextCursor);