using IbraHabra.NET.Application.Dto.Response;
using IbraHabra.NET.Application.UseCases.Client.Commands;
using IbraHabra.NET.Application.UseCases.Client.Queries;
using IbraHabra.NET.Domain.Constants.ValueObject;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace IbraHabra.NET.Adapter.Controller;

[ApiController]
[Route("api/clients")]
public class ClientController : ControllerBase
{
    private readonly IMessageBus _messageBus;

    public ClientController(IMessageBus messageBus)
    {
        _messageBus = messageBus;
    }

    /// <summary>
    /// Create a new OAuth client application
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateClient([FromBody] CreateClientCommand command)
    {

        var result = await _messageBus.InvokeAsync<ApiResult<CreateClientResponse>>(command);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetClient),new { clientId = result.Value!.Id }, result.Value)
            : StatusCode(result.StatusCode, result.Error);
    }

    /// <summary>
    /// Get client by ClientId
    /// </summary>
    [HttpGet("{clientId}")]
    public async Task<IActionResult> GetClient([FromRoute] string clientId)
    {
        var result = await _messageBus.InvokeAsync<ApiResult<GetClientResponse>>(
            new GetClientByIdQuery(clientId));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Update client auth policy
    /// </summary>
    [HttpPut("{clientId}/auth-policy")]
    public async Task<IActionResult> UpdateAuthPolicy(
        string clientId,
        [FromBody] AuthPolicy authPolicy)
    {
        var result = await _messageBus.InvokeAsync<ApiResult<string>>(
            new UpdateClientAuthPolicyCommand(clientId, authPolicy));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Get client auth policy
    /// </summary>
    [HttpGet("{clientId}/auth-policy")]
    public async Task<IActionResult> GetAuthPolicy(string clientId)
    {
        var result = await _messageBus.InvokeAsync<ApiResult<AuthPolicy>>(
            new GetClientAuthPolicyQuery(clientId));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }
}