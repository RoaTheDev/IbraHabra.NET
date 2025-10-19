namespace IbraHabra.NET.Application.Dto.Request.Client;

public record UpdateApplicationRequest(
    string? DisplayName,
    string? ApplicationType,
    string? ClientType,
    string? ConsentType,
    List<string>? RedirectUris,
    List<string>? PostLogoutRedirectUris,
    List<string>? Permissions);

public record SetStatusRequest(bool IsActive);