namespace IbraHabra.NET.Domain.Entity;

public class ProjectRole
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public Guid ProjectId { get; set; }
    public virtual Projects Project { get; set; } = null!;

    public virtual ICollection<ProjectRolePermission> ProjectRolePermissions { get; set; } =
        new List<ProjectRolePermission>();

    public virtual ICollection<ProjectMember> ProjectMembers { get; set; } = new List<ProjectMember>();
}