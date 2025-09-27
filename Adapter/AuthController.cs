using System.Security.Claims;
using IbraHabra.NET.Application.Dto.Request;
using IbraHabra.NET.Application.Dto.Response;
using IbraHabra.NET.Application.UseCases.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace IbraHabra.NET.Adapter;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IMessageBus bus) : ControllerBase
{
    [HttpPost("[action]")]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
    {
        var res = await bus.InvokeAsync<ApiResult<RegisterUserCommandResponse>>(command);
        return res.IsSuccess ? CreatedAtRoute("UserInfoEndpoint", null, res) : StatusCode(res.StatusCode, res.Error);
    }

    [HttpPost("[action]")]
    public async Task<IActionResult> Login([FromBody] LoginUserCommand command)
    {
        var res = await bus.InvokeAsync<ApiResult<LoginUserCommandResponse>>(command);
        return res.IsSuccess ? Ok(res.Value) : StatusCode(res.StatusCode, res.Error);
    }

    [HttpGet]
    public async Task<IActionResult> SendConfirmationEmail([FromBody] SendConfirmationEmailCommand command)
    {
        var res = await bus.InvokeAsync<ApiResult>(command);
        return res.IsSuccess ? Ok() : StatusCode(res.StatusCode, res.Error);
    }

    [HttpPost("[action]")]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailCommand command)
    {
        var res = await bus.InvokeAsync<ApiResult>(command);
        return res.IsSuccess ? Ok() : StatusCode(res.StatusCode, res.Error);
    }

    [HttpPost("[action]")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var res = await bus.InvokeAsync<ApiResult>(
            new LogoutUserCommand(Guid.Parse(userId), request.ClientId,
                request.RevokeAllToken));
        return res.IsSuccess ? Ok() : StatusCode(res.StatusCode, res.Error);
    }

    // [HttpPost("enable-2fa")]
    // public async Task<IResult> Enable2Fa()
    // {
    // }
}