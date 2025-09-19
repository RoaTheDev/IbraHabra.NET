namespace IbraHabra.NET.Domain.Entity;

public class ProjectRolePermission
{
    public Guid ProjectRoleId { get; set; }
    public Guid PermissionId { get; set; }

    public virtual ProjectRole ProjectRole { get; set; } = null!;
    public virtual Permission Permission { get; set; } = null!;
}