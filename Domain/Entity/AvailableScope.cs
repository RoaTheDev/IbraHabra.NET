namespace IbraHabra.NET.Domain.Entity;

public class AvailableScope
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsSystemScope { get; set; } // openid, profile, email
    public bool IsActive { get; set; } = true;
    
    public Guid? ProjectId { get; set; }
    public virtual Projects? Project { get; set; }
}