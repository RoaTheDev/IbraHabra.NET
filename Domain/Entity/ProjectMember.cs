namespace IbraHabra.NET.Domain.Entity;

public class ProjectMember
{
    public Guid ProjectId { get; set; }
    public Guid UserId { get; set; }
    public Guid ProjectRoleId { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public virtual Projects Projects { get; set; } = null!;
    public virtual User User { get; set; } = null!;
    public virtual ProjectRole ProjectRole { get; set; } = null!;
}