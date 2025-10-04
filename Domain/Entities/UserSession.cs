using IbraHabra.NET.Domain.Contract;

namespace IbraHabra.NET.Domain.Entities;

public class UserSession : IEntity<Guid>
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string SessionToken { get; set; } = Guid.NewGuid().ToString();
    public string ClientId { get; set; } = string.Empty;
    public string DeviceInfo { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastActiveAt { get; set; }
    public bool IsTrusted { get; set; } = false;
    public bool IsCurrent { get; set; } = false;
    public DateTime? TrustedAt { get; set; }
}