using Microsoft.AspNetCore.Identity;

namespace IbraHabra.NET.Domain.Entity;

public class User : IdentityUser<Guid>
{
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
}