using IbraHabra.NET.Domain.Contract;

namespace IbraHabra.NET.Domain.Entities;

public class Projects : IEntity<Guid>
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = null!;
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public string? HomePageUrl { get; set; }

    public string? AllowedCorsOrigins { get; set; }
    public bool AllowRegistration { get; set; } = true;
    public bool AllowSocialLogin { get; set; } = true;
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public virtual ICollection<ProjectRole> ProjectRoles { get; set; } = new List<ProjectRole>();

    public virtual ICollection<AvailableScope> AvailableScopes { get; set; } = new List<AvailableScope>();
    public virtual ICollection<OauthApplication> OauthApplications { get; set; } = new List<OauthApplication>();
    public virtual ICollection<ProjectMember> ProjectMembers { get; set; } = new List<ProjectMember>();
}