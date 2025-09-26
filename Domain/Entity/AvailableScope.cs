using IbraHabra.NET.Domain.Interface;

namespace IbraHabra.NET.Domain.Entity;

public class AvailableScope : IEntity<Guid>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsSystemScope { get; set; }
    public bool IsActive { get; set; } = true;
    
    public Guid? ProjectId { get; set; }
    public virtual Projects? Project { get; set; }
}