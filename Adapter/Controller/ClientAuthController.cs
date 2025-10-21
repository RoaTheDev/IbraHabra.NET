using System.Security.Claims;
using IbraHabra.NET.Application.Dto;
using IbraHabra.NET.Application.Dto.Request;
using IbraHabra.NET.Application.UseCases.Users.Commands;
using IbraHabra.NET.Application.UseCases.Users.Commands.LoginUser;
using IbraHabra.NET.Application.UseCases.Users.Commands.RegisterUser;
using IbraHabra.NET.Application.UseCases.Users.Commands.Verify2Fa;
using IbraHabra.NET.Infra.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace IbraHabra.NET.Adapter.Controller;

[ApiController]
[Route("api/[controller]")]
public class ClientAuthController : BaseApiController
{
    private readonly IMessageBus _bus;

    public ClientAuthController(IMessageBus bus)
    {
        _bus = bus;
    }

    [HttpPost("[action]")]
    [ValidateModel<RegisterUserCommand>]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
    {
        var res = await _bus.InvokeAsync<ApiResult<RegisterUserCommandResponse>>(command);
        return FromCreatedResult(res, "UserInfoEndpoint", null!);
    }

    [HttpPost("[action]")]
    [ValidateModel<LoginUserCommand>]
    public async Task<IActionResult> Login([FromBody] LoginUserCommand command)
    {
        var res = await _bus.InvokeAsync<ApiResult<LoginUserCommandResponse>>(command);
        return FromApiResult(res);
    }

    [HttpGet]
    public async Task<IActionResult> SendConfirmationEmail([FromBody] SendConfirmationEmailCommand command)
    {
        var res = await _bus.InvokeAsync<ApiResult>(command);
        return FromApiResult(res);
    }

    [HttpPost("[action]")]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailCommand command)
    {
        var res = await _bus.InvokeAsync<ApiResult>(command);
        return FromApiResult(res);
    }

    [HttpPost("[action]")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var res = await _bus.InvokeAsync<ApiResult>(
            new LogoutUserCommand(Guid.Parse(userId), request.ClientId,
                request.RevokeAllToken));
        return FromApiResult(res);
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
        var result = await _bus.InvokeAsync<ApiResult<Verify2FaResponse>>(command);
        return FromApiResult(result);
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
        var result = await _bus.InvokeAsync<ApiResult<Setup2FaInfoResponse>>(
            new Initialize2FaSetupCommand(HttpContext!.User, clientId));
        return FromApiResult(result);
    }

    /// <summary>
    /// Enable 2FA with verification code (for logged-in users)
    /// </summary>
    /// <param name="command">Authenticator verification code</param>
    /// <returns>Recovery codes</returns>
    [HttpPost("2fa/enable")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResult<Enable2FaResponse>), 200)]
    [ProducesResponseType(typeof(ApiResult<Enable2FaResponse>), 400)]
    [ProducesResponseType(typeof(ApiResult<Enable2FaResponse>), 401)]
    public async Task<IActionResult> Enable([FromBody] Enable2FaCommand command)
    {
        var result = await _bus.InvokeAsync<ApiResult<Enable2FaResponse>>(command);
        return FromApiResult(result);
    }

    /// <summary>
    /// Disable 2FA (for logged-in users)
    /// </summary>
    /// <returns>Success confirmation</returns>
    [HttpPost("2fa/disable")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResult), 200)]
    [ProducesResponseType(typeof(ApiResult), 400)]
    [ProducesResponseType(typeof(ApiResult), 401)]
    public async Task<IActionResult> Disable([FromBody] string clientId)
    {
        var result =
            await _bus.InvokeAsync<ApiResult>(
                new Disable2FaCommand(HttpContext!.User, clientId));
        return FromApiResult(result);
    }

    /// <summary>
    /// Initialize 2FA setup for policy compliance (non-authenticated users)
    /// Use this when user is blocked from login due to MFA policy requirement
    /// </summary>
    /// <param name="command">Email, password, and client credentials</param>
    /// <returns>QR code, manual key, and compliance token</returns>
    [HttpPost("2fa/compliance/initialize")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResult<Setup2FaComplianceResponse>), 200)]
    [ProducesResponseType(typeof(ApiResult<Setup2FaComplianceResponse>), 400)]
    [ProducesResponseType(typeof(ApiResult<Setup2FaComplianceResponse>), 401)]
    public async Task<IActionResult> InitializeCompliance([FromBody] Setup2FaComplianceCommand command)
    {
        var result = await _bus.InvokeAsync<ApiResult<Setup2FaComplianceResponse>>(command);
        return FromApiResult(result);
    }

    /// <summary>
    /// Enable 2FA via compliance token and complete login
    /// </summary>
    /// <param name="command">Compliance token, verification code, and client ID</param>
    /// <returns>Recovery codes and user ID</returns>
    [HttpPost("2fa/compliance/enable")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResult<Enable2FaComplianceResponse>), 200)]
    [ProducesResponseType(typeof(ApiResult<Enable2FaComplianceResponse>), 400)]
    [ProducesResponseType(typeof(ApiResult<Enable2FaComplianceResponse>), 401)]
    public async Task<IActionResult> EnableCompliance([FromBody] Enable2FaComplianceCommand command)
    {
        var result = await _bus.InvokeAsync<ApiResult<Enable2FaComplianceResponse>>(command);
        return FromApiResult(result);
    }
}