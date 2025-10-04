using System.Net;
using System.Runtime.InteropServices;
using IbraHabra.NET.Application.Dto.Response;
using IbraHabra.NET.Application.UseCases.Project.Commands.CreateProject;
using IbraHabra.NET.Application.UseCases.Project.Commands.UpdateProject;
using IbraHabra.NET.Application.UseCases.Project.Queries;
using IbraHabra.NET.Infra.Filters;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace IbraHabra.NET.Adapter.Controller;

[ApiController]
[Route("api/projects")]
public class ProjectController : ControllerBase
{
    private readonly IMessageBus _messageBus;

    public ProjectController(IMessageBus messageBus)
    {
        _messageBus = messageBus;
    }

    [ValidateModel(typeof(CreateProjectCommand))]
    [HttpPost]
    public async Task<IActionResult> CreateProject(CreateProjectCommand command)
    {
        var res = await _messageBus.InvokeAsync<ApiResult<CreateProjectResponse>>(command);
        return res.IsSuccess
            ? CreatedAtAction(nameof(GetProjectById), new { projectId = res.Value!.Id }, res.Value)
            : StatusCode(res.StatusCode, res.Error);
    }


    [HttpGet("{projectId}")]
    public async Task<IActionResult> GetProjectById([FromRoute] Guid projectId)
    {
        var res = await _messageBus.InvokeAsync<ApiResult<GetProjectByIdResponse>>(new GetProjectByIdQuery(projectId));
        return res.IsSuccess ? Ok(res.Value) : StatusCode(res.StatusCode, res.Error);
    }
    //
    // [HttpPut]
    // public async Task<IActionResult> UpdateProject([FromBody] UpdateProjectCommand command)
    // {
    //     var res = await _messageBus.
    // }
}