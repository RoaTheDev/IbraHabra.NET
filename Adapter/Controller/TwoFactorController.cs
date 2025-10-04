using IbraHabra.NET.Application.Dto.Response;
using IbraHabra.NET.Application.UseCases.Users.Commands;
using IbraHabra.NET.Application.UseCases.Users.Commands.Verify2Fa;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace IbraHabra.NET.Adapter.Controller;

/// <summary>
/// Two-Factor Authentication management endpoints
/// </summary>
[ApiController]
[Route("api/auth/2fa")]
public class TwoFactorController : ControllerBase
{
    private readonly ICommandBus _commandBus;
    private readonly IHttpContextAccessor _contextAccessor;

    public TwoFactorController(ICommandBus commandBus, IHttpContextAccessor contextAccessor)
    {
        _commandBus = commandBus;
        _contextAccessor = contextAccessor;
    }

    /// <summary>
    /// Verify 2FA code and complete login
    /// </summary>
    /// <param name="command">2FA token and verification code</param>
    /// <returns>User ID on successful verification</returns>
    [HttpPost("2fa/verify")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResult<Verify2FaResponse>), 200)]
    [ProducesResponseType(typeof(ApiResult<Verify2FaResponse>), 401)]
    public async Task<IActionResult> VerifyTwoFactor([FromBody] Verify2FaCommand command)
    {
        var result = await _commandBus.InvokeAsync<ApiResult<Verify2FaResponse>>(command);
        return result.IsSuccess ? Ok() : StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Initialize 2FA setup and get QR code (for logged-in users)
    /// </summary>
    /// <returns>QR code URI and manual entry key</returns>
    [HttpPost("initialize{clientId}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResult<Setup2FaInfoResponse>), 200)]
    [ProducesResponseType(typeof(ApiResult<Setup2FaInfoResponse>), 400)]
    [ProducesResponseType(typeof(ApiResult<Setup2FaInfoResponse>), 401)]
    public async Task<IActionResult> InitializeSetup([FromBody] string clientId)
    {
        var result = await _commandBus.InvokeAsync<ApiResult<Setup2FaInfoResponse>>(
            new Initialize2FaSetupCommand(_contextAccessor.HttpContext!.User, clientId));
        return result.IsSuccess ? Ok() : StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Enable 2FA with verification code (for logged-in users)
    /// </summary>
    /// <param name="command">Authenticator verification code</param>
    /// <returns>Recovery codes</returns>
    [HttpPost("enable")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResult<Enable2FaResponse>), 200)]
    [ProducesResponseType(typeof(ApiResult<Enable2FaResponse>), 400)]
    [ProducesResponseType(typeof(ApiResult<Enable2FaResponse>), 401)]
    public async Task<IActionResult> Enable([FromBody] Enable2FaCommand command)
    {
        var result = await _commandBus.InvokeAsync<ApiResult<Enable2FaResponse>>(command);
        return result.IsSuccess ? Ok() : StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Disable 2FA (for logged-in users)
    /// </summary>
    /// <returns>Success confirmation</returns>
    [HttpPost("disable")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResult), 200)]
    [ProducesResponseType(typeof(ApiResult), 400)]
    [ProducesResponseType(typeof(ApiResult), 401)]
    public async Task<IActionResult> Disable([FromBody] string clientId)
    {
        var result =
            await _commandBus.InvokeAsync<ApiResult>(
                new Disable2FaCommand(_contextAccessor.HttpContext!.User, clientId));
        return result.IsSuccess ? Ok() : StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Initialize 2FA setup for policy compliance (non-authenticated users)
    /// Use this when user is blocked from login due to MFA policy requirement
    /// </summary>
    /// <param name="command">Email, password, and client credentials</param>
    /// <returns>QR code, manual key, and compliance token</returns>
    [HttpPost("compliance/initialize")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResult<Setup2FaComplianceResponse>), 200)]
    [ProducesResponseType(typeof(ApiResult<Setup2FaComplianceResponse>), 400)]
    [ProducesResponseType(typeof(ApiResult<Setup2FaComplianceResponse>), 401)]
    public async Task<IActionResult> InitializeCompliance([FromBody] Setup2FaComplianceCommand command)
    {
        var result = await _commandBus.InvokeAsync<ApiResult<Setup2FaComplianceResponse>>(command);
        return result.IsSuccess ? Ok() : StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Enable 2FA via compliance token and complete login
    /// </summary>
    /// <param name="command">Compliance token, verification code, and client ID</param>
    /// <returns>Recovery codes and user ID</returns>
    [HttpPost("compliance/enable")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResult<Enable2FaComplianceResponse>), 200)]
    [ProducesResponseType(typeof(ApiResult<Enable2FaComplianceResponse>), 400)]
    [ProducesResponseType(typeof(ApiResult<Enable2FaComplianceResponse>), 401)]
    public async Task<IActionResult> EnableCompliance([FromBody] Enable2FaComplianceCommand command)
    {
        var result = await _commandBus.InvokeAsync<ApiResult<Enable2FaComplianceResponse>>(command);
        return result.IsSuccess ? Ok() : StatusCode(result.StatusCode, result);
    }
}