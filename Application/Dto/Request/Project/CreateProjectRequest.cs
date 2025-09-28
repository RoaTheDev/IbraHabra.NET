namespace IbraHabra.NET.Application.Dto.Request.Project;

public class CreateProjectRequest : BaseProjectRequest
{
    public string? LogoUrl { get; set; }
    public bool AllowRegistration { get; set; }
    public bool AllowSocialLogin { get; set; }
}