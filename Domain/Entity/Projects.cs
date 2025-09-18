namespace IbraHabra.NET.Domain.Entity;

public class Project
{
    public Guid ProjectId { get; set; } 
    public string DisplayName { get; set; } = null!;
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public string? HomePageUrl { get; set; }

    public bool AllowRegistration { get; set; } = true;
    public bool AllowSocialLogin { get; set; } = true;
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public IList<Client> Clients { get; set; } = new List<Client>();
}