using System.Linq.Expressions;
using IbraHabra.NET.Application.Dto;
using IbraHabra.NET.Application.Dto.Request.Client;
using IbraHabra.NET.Application.Dto.Response;
using IbraHabra.NET.Application.UseCases.Client.Commands.CreateClient;
using IbraHabra.NET.Application.UseCases.Client.Commands.UpdateClientAuthPolicy;
using IbraHabra.NET.Application.UseCases.Client.Queries;
using IbraHabra.NET.Application.Utils;
using IbraHabra.NET.Domain.Constants.ValueObject;
using IbraHabra.NET.Domain.Contract;
using IbraHabra.NET.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using Wolverine;

namespace IbraHabra.NET.Adapter.Controller;

[ApiController]
[Route("api/admin/apps")]
[Authorize(Policy = "AdminOnly")]
public class AdminApplicationsController : ControllerBase
{
    private readonly ICommandBus _bus;
    private readonly IOpenIddictApplicationManager _appManager;
    private readonly IRepo<OauthApplication, string> _appRepo;

    public AdminApplicationsController(
        ICommandBus bus,
        IOpenIddictApplicationManager appManager,
        IRepo<OauthApplication, string> appRepo)
    {
        _bus = bus;
        _appManager = appManager;
        _appRepo = appRepo;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClientCommand command)
    {
        var result = await _bus.InvokeAsync<ApiResult<CreateClientResponse>>(command);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetByClientId), new { clientId = result.Value!.ClientId }, result)
            : StatusCode(result.StatusCode, result);
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] Guid? projectId, [FromQuery] string? cursor,
        [FromQuery] int pageSize = 20)
    {
        if (pageSize <= 0 || pageSize > 200)
            pageSize = 20;

        Expression<Func<OauthApplication, bool>> predicate = a => true;
        if (projectId.HasValue && projectId.Value != Guid.Empty)
        {
            predicate = a => a.ProjectId == projectId.Value;
        }

        var (items, nextCursor) = await _appRepo.FindByCursorAsync(
            predicate,
            cursor,
            pageSize,
            a => a.CreatedAt,
            a => new AdminAppSummary(
                a.Id,
                a.ClientId!,
                a.ProjectId,
                a.DisplayName,
                a.ApplicationType,
                a.ClientType,
                a.ConsentType,
                a.IsActive,
                a.CreatedAt,
                a.UpdatedAt
            ),
            ascending: false);

        var res = new ListApplicationsResponse(items, nextCursor);
        var api = ApiResult<ListApplicationsResponse>.Ok(res);
        return Ok(api);
    }

    [HttpGet("{clientId}")]
    public async Task<IActionResult> GetByClientId([FromRoute] string clientId)
    {
        var result = await _bus.InvokeAsync<ApiResult<GetClientResponse>>(new GetClientByIdQuery(clientId));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [HttpPut("{clientId}")]
    public async Task<IActionResult> Update([FromRoute] string clientId, [FromBody] UpdateApplicationRequest request)
    {
        var app = await _appRepo.GetViaConditionAsync(a => a.ClientId == clientId);
        if (app == null)
            return StatusCode(404, ApiResult.Fail(404, "Client not found."));

        var descriptor = new OpenIddictApplicationDescriptor();
        await _appManager.PopulateAsync(descriptor, app);

        if (request.DisplayName is not null)
            descriptor.DisplayName = request.DisplayName;
        if (request.ApplicationType is not null)
            descriptor.ApplicationType = request.ApplicationType;
        if (request.ClientType is not null)
            descriptor.ClientType = request.ClientType;
        if (request.ConsentType is not null)
            descriptor.ConsentType = request.ConsentType;

        if (request.RedirectUris is not null)
        {
            descriptor.RedirectUris.Clear();
            foreach (var uriStr in request.RedirectUris)
            {
                if (!AuthUtils.TryCreateUri(uriStr, out var uri))
                    return StatusCode(400, ApiResult.Fail(400, $"Invalid redirect URI: {uriStr}"));
                descriptor.RedirectUris.Add(uri!);
            }
        }

        if (request.PostLogoutRedirectUris is not null)
        {
            descriptor.PostLogoutRedirectUris.Clear();
            foreach (var uriStr in request.PostLogoutRedirectUris)
            {
                if (!AuthUtils.TryCreateUri(uriStr, out var uri))
                    return StatusCode(400, ApiResult.Fail(400, $"Invalid post logout redirect URI: {uriStr}"));
                descriptor.PostLogoutRedirectUris.Add(uri!);
            }
        }

        if (request.Permissions is not null)
        {
            descriptor.Permissions.Clear();
            foreach (var p in request.Permissions)
            {
                if (!string.IsNullOrWhiteSpace(p))
                    descriptor.Permissions.Add(p);
            }
        }

        await _appManager.UpdateAsync(app, descriptor);
        await _appRepo.UpdateAsync(clientId,
            a => a.SetProperty(x => x.UpdatedAt, DateTime.UtcNow));

        return Ok(ApiResult.Ok());
    }

    // Admin: Set active status
    [HttpPut("{clientId}/status")]
    public async Task<IActionResult> SetStatus([FromRoute] string clientId, [FromBody] SetStatusRequest request)
    {
        var updated = await _appRepo.UpdateAsync(
            clientId,
            a => a
                .SetProperty(x => x.IsActive, request.IsActive)
                .SetProperty(x => x.UpdatedAt, DateTime.UtcNow));

        if (updated == 0)
            return StatusCode(404, ApiResult.Fail(404, "Client not found."));

        return Ok(ApiResult.Ok());
    }

    // Admin: Rotate client secret and return the new secret (one-time)
    [HttpPost("{clientId}/rotate-secret")]
    public async Task<IActionResult> RotateSecret([FromRoute] string clientId)
    {
        var app = await _appRepo.GetViaConditionAsync(a => a.ClientId == clientId);
        if (app == null)
            return StatusCode(404, ApiResult.Fail(404, "Client not found."));

        var newSecret = AuthUtils.GenerateSecureSecret();

        var descriptor = new OpenIddictApplicationDescriptor();
        await _appManager.PopulateAsync(descriptor, app);
        descriptor.ClientSecret = newSecret;

        await _appManager.UpdateAsync(app, descriptor);
        await _appRepo.UpdateAsync(clientId,
            a => a.SetProperty(x => x.UpdatedAt, DateTime.UtcNow));

        var payload = new RotateSecretResponse(clientId, newSecret);
        return Ok(ApiResult<RotateSecretResponse>.Ok(payload));
    }

    // Admin: Update auth policy (reuses existing command)
    [HttpPut("{clientId}/auth-policy")]
    public async Task<IActionResult> UpdateAuthPolicy([FromRoute] string clientId, [FromBody] AuthPolicy policy)
    {
        var result = await _bus.InvokeAsync<ApiResult<string>>(new UpdateClientAuthPolicyCommand(clientId, policy));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    // Admin: Get auth policy (reuses existing query)
    [HttpGet("{clientId}/auth-policy")]
    public async Task<IActionResult> GetAuthPolicy([FromRoute] string clientId)
    {
        var result = await _bus.InvokeAsync<ApiResult<AuthPolicy>>(new GetClientAuthPolicyQuery(clientId));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    // Admin: Delete application and associated artifacts
    [HttpDelete("{clientId}")]
    public async Task<IActionResult> Delete([FromRoute] string clientId)
    {
        var app = await _appRepo.GetViaConditionAsync(a => a.ClientId == clientId);
        if (app == null)
            return StatusCode(404, ApiResult.Fail(404, "Client not found."));

        await _appManager.DeleteAsync(app);
        return Ok(ApiResult.Ok());
    }
}