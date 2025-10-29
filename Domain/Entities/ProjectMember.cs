using IbraHabra.NET.Domain.Contract;

namespace IbraHabra.NET.Domain.Entities;

// Composite key
public record ProjectMemberId(Guid ProjectId, Guid UserId);

public class ProjectMember : IEntity<ProjectMemberId>
{
    private ProjectMemberId? _id;
    public ProjectMemberId Id => _id ??= new(ProjectId, UserId);
    public Guid ProjectId { get; set; }
    public Guid UserId { get; set; }
    public Guid ProjectRoleId { get; set; }

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public virtual Projects Projects { get; set; } = null!;
    public virtual User User { get; set; } = null!;
    public virtual ProjectRole ProjectRole { get; set; } = null!;
}