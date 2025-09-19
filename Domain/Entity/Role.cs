using Microsoft.AspNetCore.Identity;

namespace IbraHabra.NET.Domain.Entity;

public class Role : IdentityRole<Guid>
{
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; }
}