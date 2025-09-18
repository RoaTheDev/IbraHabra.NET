using Microsoft.AspNetCore.Identity;

namespace IbraHabra.NET.Domain.Entity;

public class Role : IdentityRole<int>
{
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedBy { get; set; }
}