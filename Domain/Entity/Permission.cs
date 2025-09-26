using IbraHabra.NET.Domain.Interface;

namespace IbraHabra.NET.Domain.Entity;

public class Permission : IEntity<Guid>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
}