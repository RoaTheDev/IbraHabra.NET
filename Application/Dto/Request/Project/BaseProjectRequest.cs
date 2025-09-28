namespace IbraHabra.NET.Application.Dto.Request.Project;

public class BaseProjectRequest
{
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? HomePageUrl { get; set; }
}