using IbraHabra.NET.Domain.SharedKernel.Interface;
using Microsoft.AspNetCore.Identity;

namespace IbraHabra.NET.Domain.Entity;

public class User : IdentityUser<Guid>, IEntity<Guid>
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; } 
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public virtual ICollection<UserAuditTrail> UserAuditTrails { get; set; } = new List<UserAuditTrail>();
}