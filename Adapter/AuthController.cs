using IbraHabra.NET.Application.UseCases.Users;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace IbraHabra.NET.Adapter;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IMessageBus bus)
{
    private readonly IMessageBus _bus = bus;

    [HttpPost("[action]")]
    public async Task<IResult> Register(RegisterUserCommand command)
    {
    }

    [HttpPost("[action]")]
    public async Task<IResult> Login()
    {
    }

    [HttpGet]
    public async Task<IResult> SendConfirmationEmail()
    {
        
    }

    [HttpPost]
    public async Task<IResult> ConfirmEmail()
    {
        
    }

    [HttpPost]
    public async Task<IResult> Enable2Fa()
    {
        
    }
}