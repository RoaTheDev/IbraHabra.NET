namespace IbraHabra.NET.Application.UseCases.Project;

public record BaseProjectCommand(
    string DisplayName,
    string? Description,
    string? HomePageUrl
);