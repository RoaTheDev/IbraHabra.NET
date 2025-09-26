using IbraHabra.NET.Domain.Interface;

namespace IbraHabra.NET.Domain.Entity;

public record ProjectRolePermissionId(Guid ProjectRoleId, Guid PermissionId);

public class ProjectRolePermission : IEntity<ProjectRolePermissionId>
{
    private ProjectRolePermissionId? _id;
    public ProjectRolePermissionId Id => _id ??= new(ProjectRoleId, PermissionId);
    public Guid ProjectRoleId { get; set; }
    public Guid PermissionId { get; set; }

    public virtual ProjectRole ProjectRole { get; set; } = null!;
    public virtual Permission Permission { get; set; } = null!;
}