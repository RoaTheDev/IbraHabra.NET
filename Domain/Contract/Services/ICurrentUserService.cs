namespace IbraHabra.NET.Domain.Contract.Services;

public interface ICurrentUserService
{
    Guid UserId { get; }
    bool IsAuthenticated { get; }
    string? Email { get; }
}