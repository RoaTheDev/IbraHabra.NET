namespace IbraHabra.NET.Application.Dto.Request.User;

public record RegisterRequest(
    string Email,
    string? FirstName,
    string? LastName,
    string Password);