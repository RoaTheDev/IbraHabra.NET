namespace IbraHabra.NET.Domain.SharedKernel.Interface.Services;

public interface ICurrentUserService
{
    Guid UserId { get; }
    bool IsAuthenticated { get; }
    string? Email { get; }
}