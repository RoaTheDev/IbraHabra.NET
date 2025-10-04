using IbraHabra.NET.Application.Dto.Request.Project;
using IbraHabra.NET.Application.UseCases.Project;
using IbraHabra.NET.Application.UseCases.Project.Commands.CreateProject;
using IbraHabra.NET.Application.UseCases.Project.Commands.UpdateProject;
using Mapster;

namespace IbraHabra.NET.Application.Mapper;

public class ProjectMapper
{
    public static void MappingConfig()
    {
        TypeAdapterConfig<UpdateProjectRequest, UpdateProjectCommand>
            .NewConfig();
        TypeAdapterConfig<CreateProjectRequest, CreateProjectCommand>.NewConfig();
        
    }
}