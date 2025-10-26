using IbraHabra.NET.Application.Dto;
using IbraHabra.NET.Application.UseCases.Admin.Commands.AssignRole;
using IbraHabra.NET.Application.UseCases.Admin.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace IbraHabra.NET.Adapter.Controller;

/// <summary>
/// Admin controller for managing system-wide roles (Admin, SuperAdmin, etc.)
/// These are Identity roles, not project-specific roles
/// </summary>
[ApiController]
[Route("api/admin/roles")]
[Authorize(Policy = "AdminOnly")]
public class AdminRolesController : BaseApiController
{
    private readonly ICommandBus _bus;

    public AdminRolesController(ICommandBus bus)
    {
        _bus = bus;
    }

    // /// <summary>
    // /// Create a new system role
    // /// </summary>
    // [HttpPost]
    // [Authorize(Roles = "SuperAdmin")]
    // public async Task<IActionResult> CreateRole([FromBody] CreateRoleCommand command)
    // {
    //     var result = await _bus.InvokeAsync<ApiResult<CreateRoleResponse>>(command);
    //     return FromCreatedResult(result,CreatedAtAction(nameof(GetRole), new { roleId = result.Data! })
    // }

    /// <summary>
    /// Get all system roles
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllRoles()
    {
        var result = await _bus.InvokeAsync<ApiResult<List<RoleResponse>>>(new GetAllRolesQuery());
        return FromApiResult(result);
    }

    /// <summary>
    /// Get a specific role by ID
    /// </summary>
    [HttpGet("{roleId}")]
    public async Task<IActionResult> GetRole([FromRoute] Guid roleId)
    {
        var result = await _bus.InvokeAsync<ApiResult<RoleResponse>>(new GetRoleByIdQuery(roleId));
        return FromApiResult(result);
    }

    /// <summary>
    /// Assign a system role to a user
    /// </summary>
    [HttpPost("assign")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleCommand command)
    {
        var result = await _bus.InvokeAsync<ApiResult>(command);
        return FromApiResult(result);
    }

    // /// <summary>
    // /// Remove a system role from a user
    // /// </summary>
    // [HttpPost("remove")]
    // [Authorize(Roles = "SuperAdmin")]
    // public async Task<IActionResult> RemoveRole([FromBody] RemoveRoleCommand command)
    // {
    //     var result = await _bus.InvokeAsync<ApiResult>(command);
    //     return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    // }

    /// <summary>
    /// Get all users with a specific role
    /// </summary>
    [HttpGet("{roleId}/users")]
    public async Task<IActionResult> GetUsersInRole([FromRoute] Guid roleId)
    {
        var result = await _bus.InvokeAsync<ApiResult<List<UserRoleResponse>>>(new GetUsersInRoleQuery(roleId));
        return FromApiResult(result);
    }

    /// <summary>
    /// Get all roles for a specific user
    /// </summary>
    [HttpGet("users/{userId}")]
    public async Task<IActionResult> GetUserRoles([FromRoute] Guid userId)
    {
        var result = await _bus.InvokeAsync<ApiResult<List<RoleResponse>>>(new GetUserRolesQuery(userId));
        return FromApiResult(result);
    }
}