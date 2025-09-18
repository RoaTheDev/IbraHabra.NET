namespace IbraHabra.NET.Domain.Entity;

public class ProjectMember
{
    public Guid ProjectId { get; set; }
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public Project Project { get; set; } = null!;
    public User User { get; set; } = null!;
}