using IbraHabra.NET.Domain.Contract;

namespace IbraHabra.NET.Domain.Entities;

public class ClientOrigin : IEntity<Guid>
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public string Origin { get; set; } = null!;
    public bool IsActive { get; set; } = true;

    public string Type { get; set; } = nameof(OriginType.Client);
    public OauthApplication Client { get; set; } = null!;
}

public enum OriginType
{
    Project,
    Client
}