using System.Security.Claims;
using FluentValidation;
using IbraHabra.NET.Application.Dto.Request;
using IbraHabra.NET.Application.Dto.Response;
using IbraHabra.NET.Application.UseCases.Users.Commands;
using IbraHabra.NET.Application.UseCases.Users.Commands.LoginUser;
using IbraHabra.NET.Application.UseCases.Users.Commands.RegisterUser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace IbraHabra.NET.Adapter.Controller;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMessageBus _bus;
    private readonly IValidator<LoginUserCommand> _loginValidator;
    private readonly IValidator<RegisterUserCommand> _registerValidator;

    public AuthController(IMessageBus bus, IValidator<LoginUserCommand> loginValidator,
        IValidator<RegisterUserCommand> registerValidator)
    {
        _bus = bus;
        _loginValidator = loginValidator;
        _registerValidator = registerValidator;
    }

    [HttpPost("[action]")]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
    {
        var validationResult = await _registerValidator.ValidateAsync(command);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);
        var res = await _bus.InvokeAsync<ApiResult<RegisterUserCommandResponse>>(command);
        return res.IsSuccess ? CreatedAtRoute("UserInfoEndpoint", null, res) : StatusCode(res.StatusCode, res.Error);
    }

    [HttpPost("[action]")]
    public async Task<IActionResult> Login([FromBody] LoginUserCommand command)
    {
        var validationResult = await _loginValidator.ValidateAsync(command);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);
        var res = await _bus.InvokeAsync<ApiResult<LoginUserCommandResponse>>(command);
        return res.IsSuccess ? Ok(res.Value) : StatusCode(res.StatusCode, res.Error);
    }

    [HttpGet]
    public async Task<IActionResult> SendConfirmationEmail([FromBody] SendConfirmationEmailCommand command)
    {
        var res = await _bus.InvokeAsync<ApiResult>(command);
        return res.IsSuccess ? Ok() : StatusCode(res.StatusCode, res.Error);
    }

    [HttpPost("[action]")]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailCommand command)
    {
        var res = await _bus.InvokeAsync<ApiResult>(command);
        return res.IsSuccess ? Ok() : StatusCode(res.StatusCode, res.Error);
    }

    [HttpPost("[action]")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var res = await _bus.InvokeAsync<ApiResult>(
            new LogoutUserCommand(Guid.Parse(userId), request.ClientId,
                request.RevokeAllToken));
        return res.IsSuccess ? Ok() : StatusCode(res.StatusCode, res.Error);
    }
}