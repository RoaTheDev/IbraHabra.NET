using System.Linq.Expressions;
using IbraHabra.NET.Application.Dto;
using IbraHabra.NET.Application.Dto.Response;
using IbraHabra.NET.Application.UseCases.Project.Commands.CreateProject;
using IbraHabra.NET.Application.UseCases.Project.Queries;
using IbraHabra.NET.Domain.Contract;
using IbraHabra.NET.Domain.Entities;
using IbraHabra.NET.Infra.Filters;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace IbraHabra.NET.Adapter.Controller;

[ApiController]
[Route("api/projects")]
public class ProjectController(IMessageBus messageBus) : BaseApiController
{
    [ValidateModel<CreateProjectCommand>]
    [HttpPost]
    public async Task<IActionResult> CreateProject(CreateProjectCommand command)
    {
        var result = await messageBus.InvokeAsync<ApiResult<CreateProjectResponse>>(command);
        return FromApiResult(result);
    }


    [HttpGet("{projectId}")]
    public async Task<IActionResult> GetProjectById([FromRoute] Guid projectId)
    {
        var result =
            await messageBus.InvokeAsync<ApiResult<GetProjectByIdResponse>>(new GetProjectByIdQuery(projectId));
        return FromApiResult(result);
    }

    //
    // [HttpPut]
    // public async Task<IActionResult> UpdateProject([FromBody] UpdateProjectCommand command)
    // {
    //     var res = await _messageBus.
    // }
}