using Asp.Versioning;
using IbraHabra.NET.Application.Dto;
using IbraHabra.NET.Application.UseCases.Admin.Commands;
using IbraHabra.NET.Application.UseCases.Admin.Commands.Confirm2FaAdmin;
using IbraHabra.NET.Application.UseCases.Admin.Commands.CreateUser;
using IbraHabra.NET.Application.UseCases.Admin.Commands.Disable2FaAdmin;
using IbraHabra.NET.Application.UseCases.Admin.Commands.Enable2FaAdmin;
using IbraHabra.NET.Application.UseCases.Admin.Commands.LoginAdmin;
using IbraHabra.NET.Application.UseCases.Admin.Commands.RefreshAdminToken;
using IbraHabra.NET.Application.UseCases.Admin.Commands.Verify2FaAdmin;
using IbraHabra.NET.Application.UseCases.Admin.Queries;
using IbraHabra.NET.Infra.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace IbraHabra.NET.Adapter.Controller;

[ApiController]
[Route("v{version:apiVersion}/api/admin/auth")]
[EnableCors("AdminPolicy")]
[Authorize(Policy = "AdminOnly")]
[ApiVersion("1.0")]
public class AdminAuthController : BaseApiController
{
    private readonly IMessageBus _bus;

    public AdminAuthController(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>
    /// Admin login endpoint - authenticates admin users and returns JWT token
    /// If 2FA is enabled, returns RequiresTwoFactor=true
    /// </summary>
    [HttpPost("login")]
    [ValidateModel<LoginAdminCommand>]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginAdminCommand command)
    {
        var result = await _bus.InvokeAsync<ApiResult<LoginAdminCommandResponse>>(command);
        return FromApiResult(result);
    }

    [HttpPost("register")]
    [ValidateModel<CreateUserCommand>]
    public async Task<IActionResult> Register([FromBody] CreateUserCommand command)
    {
        var result = await _bus.InvokeAsync<ApiResult>(command);
        return FromApiResult(result);
    }

    /// <summary>
    /// Enable 2FA - Step 1: Generate QR code and recovery codes
    /// Returns shared key, authenticator URI, and recovery codes
    /// </summary>
    [HttpPost("2fa/enable")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Enable2Fa()
    {
        var result = await _bus.InvokeAsync<ApiResult<Enable2FaAdminResponse>>(new Enable2FaAdminCommand(HttpContext));
        return FromApiResult(result);
    }

    /// <summary>
    /// Enable 2FA - Step 2: Confirm with verification code from authenticator app
    /// </summary>
    [HttpPost("2fa/enable/confirm")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> ConfirmEnable2Fa([FromBody] ConfirmEnable2FaAdminCommand command)
    {
        var result = await _bus.InvokeAsync<ApiResult<ConfirmEnable2FaAdminResponse>>(command);
        return FromApiResult(result);
    }

    /// <summary>
    /// Disable 2FA - Requires password confirmation
    /// </summary>
    [HttpPost("2fa/disable")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Disable2Fa([FromBody] Disable2FaAdminRequest request)
    {
        var command = new Disable2FaAdminCommand(request.Password, HttpContext);
        var result = await _bus.InvokeAsync<ApiResult<Disable2FaAdminResponse>>(command);
        return FromApiResult(result);
    }

    /// <summary>
    /// Verify 2FA code for admin login
    /// </summary>
    [HttpPost("verify-2fa")]
    [AllowAnonymous]
    public async Task<IActionResult> Verify2Fa([FromBody] Verify2FaAdminCommand command)
    {
        var result = await _bus.InvokeAsync<ApiResult<Verify2FaAdminCommandResponse>>(command);
        return FromApiResult(result);
    }

    /// <summary>
    /// Refresh admin JWT token
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken()
    {
        var authHeader = HttpContext.Request.Headers.Authorization.ToString();
        var token = authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? authHeader.Substring("Bearer ".Length).Trim()
            : null;

        if (string.IsNullOrEmpty(token))
            return Unauthorized(new { message = "No token provided" });

        var result = await _bus.InvokeAsync<ApiResult<RefreshAdminTokenResponse>>(
            new RefreshAdminTokenCommand(token));

        return FromApiResult(result);
    }

    /// <summary>
    /// Get current admin user information
    /// </summary>
    [HttpGet("me")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetCurrentAdmin()
    {
        var result = await _bus.InvokeAsync<ApiResult<AdminUserInfoResponse>>(new GetAdminUserInfoQuery());
        return FromApiResult(result);
    }

    /// <summary>
    /// Verify admin token validity
    /// </summary>
    [HttpGet("verify")]
    [Authorize(Policy = "AdminOnly")]
    public IActionResult VerifyToken()
    {
        return Ok(ApiResult<object>.Ok(new { valid = true, message = "Token is valid" }));
    }

    /// <summary>
    /// Admin logout - invalidates the current session
    /// </summary>
    [HttpPost("logout")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Logout()
    {
        var result = await _bus.InvokeAsync<ApiResult>(new LogoutAdminCommand());

        return FromApiResult(result);
    }
}