using FluentValidation;
using IbraHabra.NET.Application.Dto.Response;
using IbraHabra.NET.Application.UseCases.Admin.Commands.LoginAdmin;
using IbraHabra.NET.Application.UseCases.Admin.Commands.RefreshAdminToken;
using IbraHabra.NET.Application.UseCases.Admin.Commands.Verify2FaAdmin;
using IbraHabra.NET.Application.UseCases.Admin.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace IbraHabra.NET.Adapter.Controller;

[ApiController]
[Route("api/admin/auth")]
public class AdminAuthController : ControllerBase
{
    private readonly ICommandBus _bus;
    private readonly IValidator<LoginAdminCommand> _loginValidator;

    public AdminAuthController(ICommandBus bus, IValidator<LoginAdminCommand> loginValidator)
    {
        _bus = bus;
        _loginValidator = loginValidator;
    }

    /// <summary>
    /// Admin login endpoint - authenticates admin users and returns JWT token
    /// If 2FA is enabled, returns RequiresTwoFactor=true
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginAdminCommand command)
    {
        var validationResult = await _loginValidator.ValidateAsync(command);
        if (!validationResult.IsValid)
            return BadRequest(ApiResult.Fail(400, validationResult.Errors.First().ErrorMessage));

        var result = await _bus.InvokeAsync<ApiResult<LoginAdminCommandResponse>>(command);
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Verify 2FA code for admin login
    /// </summary>
    [HttpPost("verify-2fa")]
    public async Task<IActionResult> Verify2Fa([FromBody] Verify2FaAdminCommand command)
    {
        var result = await _bus.InvokeAsync<ApiResult<Verify2FaAdminCommandResponse>>(command);
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Refresh admin JWT token
    /// </summary>
    [HttpPost("refresh")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshAdminTokenCommand command)
    {
        var result = await _bus.InvokeAsync<ApiResult<RefreshAdminTokenResponse>>(command);
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Get current admin user information
    /// </summary>
    [HttpGet("me")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> GetCurrentAdmin()
    {
        var result = await _bus.InvokeAsync<ApiResult<AdminUserInfoResponse>>(new GetAdminUserInfoQuery());
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Verify admin token validity
    /// </summary>
    [HttpGet("verify")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public IActionResult VerifyToken()
    {
        return Ok(ApiResult<object>.Ok(new { valid = true, message = "Token is valid" }));
    }

    /// <summary>
    /// Admin logout - invalidates the current session
    /// </summary>
    [HttpPost("logout")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public IActionResult Logout()
    {
        // For JWT-based auth, logout is typically handled client-side by removing the token
        // You can add additional server-side logic here if needed (e.g., token blacklisting)
        return Ok(ApiResult.Ok());
    }
}
