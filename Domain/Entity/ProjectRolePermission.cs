namespace IbraHabra.NET.Domain.Entity;

public class ProjectRolePermission
{
    public Guid ProjectId { get; set; }
    public Guid RoleId { get; set; }
    public string Permission { get; set; } = null!;

    public Project Project { get; set; } = null!;
    public Role Role { get; set; } = null!;
}