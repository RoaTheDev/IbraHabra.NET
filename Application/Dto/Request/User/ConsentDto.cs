namespace IbraHabra.NET.Application.Dto.Request.User;

public class ConsentDto
{
    public string ApplicationName { get; set; } = string.Empty;
    public string? Scope { get; set; }
    public List<ScopeDescription> RequestedScopes { get; set; } = new();
}

public class ScopeDescription
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class ConsentActionModel
{
    public string Action { get; set; } = string.Empty;
}