namespace IbraHabra.NET.Application.Dto.Request.Project;

public class UpdateProjectRequest : BaseProjectRequest
{
    public bool AllowRegistration { get; set; }
    public bool AllowSocialLogin { get; set; }
}