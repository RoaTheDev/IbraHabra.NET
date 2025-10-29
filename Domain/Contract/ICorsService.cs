namespace IbraHabra.NET.Domain.Contract;

public interface ICorsService
{
    Task<bool> IsOriginAllowedAsync(string origin, Guid? projectId = null);
    Task<bool> IsOriginAllowedForClientAsync(string origin, string clientId);
}