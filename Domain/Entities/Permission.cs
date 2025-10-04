using IbraHabra.NET.Domain.Contract;

namespace IbraHabra.NET.Domain.Entities;

public class Permission : IEntity<Guid>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
}