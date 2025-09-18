namespace IbraHabra.NET.Domain.Entity;

public class UserAuditTrail
{
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string IpAddressHash { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty; // State/Region
        public double? Latitude { get; set; } // For distance calculation
        public double? Longitude { get; set; }
        public DateTime LoginAt { get; set; } = DateTime.UtcNow;
        public string? ClientId { get; set; }
        public bool IsSuccessful { get; set; }
        public string? FailureReason { get; set; }
        public bool IsAlertTriggered { get; set; } = false; // ← New: mark if alert fired
        public string? AlertType { get; set; } // e.g., "ImpossibleTravel", "BruteForce"
}