using IbraHabra.NET.Application.Dto;
using IbraHabra.NET.Application.Dto.Request.Client;
using IbraHabra.NET.Application.UseCases.Client.Commands;
using IbraHabra.NET.Application.UseCases.Client.Commands.CreateClient;
using IbraHabra.NET.Application.UseCases.Client.Commands.UpdateClientAuthPolicy;
using IbraHabra.NET.Application.UseCases.Client.Commands.UpdateClientMetadata;
using IbraHabra.NET.Application.UseCases.Client.Queries;
using IbraHabra.NET.Domain.Constants.Values;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace IbraHabra.NET.Adapter.Controller;

[ApiController]
[Route("api/clients")]
[Authorize(Policy = "AdminPolicy")]
public class ClientController : BaseApiController
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
        return FromCreatedResult(
            result,
            nameof(GetClient),
            new { clientId = result.Data!.ClientId }
        );
    }

    /// <summary>
    /// Get client by ClientId
    /// </summary>
    [HttpGet("{clientId}")]
    public async Task<IActionResult> GetClient([FromRoute] string clientId)
    {
        var result = await _messageBus.InvokeAsync<ApiResult<GetClientResponse>>(
            new GetClientByIdQuery(clientId));
        return FromApiResult(result);
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
        return FromApiResult(result);
    }

    /// <summary>
    /// Get client auth policy
    /// </summary>
    [HttpGet("{clientId}/auth-policy")]
    public async Task<IActionResult> GetAuthPolicy(string clientId)
    {
        var result = await _messageBus.InvokeAsync<ApiResult<AuthPolicy>>(
            new GetClientAuthPolicyQuery(clientId));
        return FromApiResult(result);
    }

    [HttpPut("{clientId}/metadata")]
    public async Task<IActionResult> UpdateClientMetadata([FromRoute] string clientId,
        [FromBody] UpdateClientMetadataRequest request)
    {
        var res = await _messageBus.InvokeAsync<ApiResult<string>>(new UpdateClientMetadataCommand(clientId,
            request.DisplayName, request.ApplicationType, request.ConsentType));
        return FromApiResult(res);
    }

    [HttpDelete("{clientId}")]
    public async Task<IActionResult> Delete([FromRoute] string clientId)
    {
        var command = new DeleteClientCommand(clientId);
        var res = await _messageBus.InvokeAsync<ApiResult>(command);
        return FromApiResult(result: res);
    }

    [HttpPost("{clientId}/rotate-secret")]
    public async Task<IActionResult> RotateSecret([FromRoute] string clientId)
    {
        var command = new RotateSecretCommand(clientId);
        var res = await _messageBus.InvokeAsync<ApiResult<RotateSecretResponse>>(command);
        return FromApiResult(result: res);
    }

    [HttpPatch("{clientId}")]
    public async Task<IActionResult> Update([FromRoute] string clientId, [FromBody] UpdateApplicationRequest request)
    {
        var command = (clientId, request).Adapt<PatchClientCommand>();
        var res = await _messageBus.InvokeAsync<ApiResult>(command);
        return FromApiResult(res);
    }


    // [HttpPut("{clientId}/status")]
    // public async Task<IActionResult> SetStatus([FromRoute] string clientId, [FromBody] SetStatusRequest request)
    // {        var updated = await _appRepo.UpdateAsync(
    //         clientId,
    //         a => a
    //             .SetProperty(x => x.IsActive, request.IsActive)
    //             .SetProperty(x => x.UpdatedAt, DateTime.UtcNow));
    //
    //     if (updated == 0)
    //         return StatusCode(ApiErrors.OAuthApplication.NotFound());
    //
    //     return Ok(ApiResult.Ok());
    //
    // }

    // [HttpPut("{clientId}/permissions")]
    // public async Task<IActionResult> UpdateClientPermission([FromRoute] string clientId,
    //     [FromBody] UpdateClientPermission)
    // {
    // }
}