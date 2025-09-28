namespace IbraHabra.NET.Application.Dto.Request;

public record RegisterRequest(
    string Email,
    string? FirstName,
    string? LastName,
    string Password,
    string ClientId);