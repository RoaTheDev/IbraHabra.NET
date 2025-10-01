namespace IbraHabra.NET.Infra.Persistent.Projections;

public record AuthPolicyProjections(string? Properties);

public record AuthPolicyAndNameProjection(string? Properties,string? Name);

public record AuthPolicyAndRedirectUriProjections(string? Properties,string? RedirectUris);
