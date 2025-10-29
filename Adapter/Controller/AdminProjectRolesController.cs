using IbraHabra.NET.Application.Dto;
using IbraHabra.NET.Application.UseCases.Admin.Commands.AssignProjectRole;
using IbraHabra.NET.Application.UseCases.Admin.Commands.RemoveProjectRole;
using IbraHabra.NET.Application.UseCases.Admin.Commands.UpdateProjectRole;
using IbraHabra.NET.Application.UseCases.Admin.Queries;
using IbraHabra.NET.Application.UseCases.Project.CreateProjectRole;
using IbraHabra.NET.Application.UseCases.Project.DeleteProjectRole;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace IbraHabra.NET.Adapter.Controller;

/// <summary>
/// Admin controller for managing project-specific roles
/// ProjectRoles are scoped to individual projects and can have permissions
/// </summary>
[ApiController]
[Route("api/admin/projects/{projectId}/roles")]
[Authorize(Policy = "AdminOnly")]
[EnableCors("AdminPolicy")] 
public class AdminProjectRolesController : BaseApiController
{
    private readonly ICommandBus _bus;

    public AdminProjectRolesController(ICommandBus bus)
    {
        _bus = bus;
    }

    /// <summary>
    /// Create a new project role
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateProjectRole(
        [FromRoute] Guid projectId,
        [FromBody] CreateProjectRoleRequest request)
    {
        var command = new CreateProjectRoleCommand(
            projectId,
            request.Name,
            request.Description,
            request.PermissionIds);

        var result = await _bus.InvokeAsync<ApiResult<CreateProjectRoleResponse>>(command);
        return FromCreatedResult(result, nameof(GetProjectRole),
            new { projectId, roleId = result.Data!.ProjectRoleId });
    }

    /// <summary>
    /// Get all roles for a project
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetProjectRoles([FromRoute] Guid projectId)
    {
        var result = await _bus.InvokeAsync<ApiResult<List<ProjectRoleResponse>>>(
            new GetProjectRolesQuery(projectId));
        return FromApiResult(result);
    }

    /// <summary>
    /// Get a specific project role
    /// </summary>
    [HttpGet("{roleId}")]
    public async Task<IActionResult> GetProjectRole([FromRoute] Guid projectId, [FromRoute] Guid roleId)
    {
        var result = await _bus.InvokeAsync<ApiResult<ProjectRoleResponse>>(
            new GetProjectRoleByIdQuery(projectId, roleId));
        return FromApiResult(result);
    }

    /// <summary>
    /// Update a project role
    /// </summary>
    [HttpPut("{roleId}")]
    public async Task<IActionResult> UpdateProjectRole(
        [FromRoute] Guid projectId,
        [FromRoute] Guid roleId,
        [FromBody] UpdateProjectRoleRequest request)
    {
        var command = new UpdateProjectRoleCommand(
            projectId,
            roleId,
            request.Name,
            request.Description,
            request.PermissionIds);

        var result = await _bus.InvokeAsync<ApiResult>(command);
        return FromApiResult(result);
    }

    /// <summary>
    /// Delete a project role
    /// </summary>
    [HttpDelete("{roleId}")]
    public async Task<IActionResult> DeleteProjectRole([FromRoute] Guid projectId, [FromRoute] Guid roleId)
    {
        var result = await _bus.InvokeAsync<ApiResult>(new DeleteProjectRoleCommand(projectId, roleId));
        return FromApiResult(result);
    }

    /// <summary>
    /// Assign a project role to a user (make them a project member)
    /// </summary>
    [HttpPost("{roleId}/assign")]
    public async Task<IActionResult> AssignProjectRole(
        [FromRoute] Guid projectId,
        [FromRoute] Guid roleId,
        [FromBody] AssignProjectRoleRequest request)
    {
        var command = new AssignProjectRoleCommand(projectId, request.UserId, roleId);
        var result = await _bus.InvokeAsync<ApiResult>(command);
        return FromApiResult(result);
    }

    /// <summary>
    /// Remove a user from a project (remove project role)
    /// </summary>
    [HttpPost("{roleId}/remove")]
    public async Task<IActionResult> RemoveProjectRole(
        [FromRoute] Guid projectId,
        [FromRoute] Guid roleId,
        [FromBody] RemoveProjectRoleRequest request)
    {
        var command = new RemoveProjectRoleCommand(projectId, request.UserId);
        var result = await _bus.InvokeAsync<ApiResult>(command);

        return FromApiResult(result);
    }

    /// <summary>
    /// Get all members of a project with their roles
    /// </summary>
    [HttpGet("members")]
    public async Task<IActionResult> GetProjectMembers([FromRoute] Guid projectId)
    {
        var result = await _bus.InvokeAsync<ApiResult<List<ProjectMemberResponse>>>(
            new GetProjectMembersQuery(projectId));

        return FromApiResult(result);
    }

    // DTOs
    public record CreateProjectRoleRequest(string Name, string Description, List<Guid>? PermissionIds);

    public record UpdateProjectRoleRequest(string? Name, string? Description, List<Guid>? PermissionIds);

    public record AssignProjectRoleRequest(Guid UserId);

    public record RemoveProjectRoleRequest(Guid UserId);
}