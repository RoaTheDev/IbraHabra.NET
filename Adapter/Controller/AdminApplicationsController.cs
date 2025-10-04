using System.Security.Cryptography;
using IbraHabra.NET.Application.Dto.Response;
using IbraHabra.NET.Application.UseCases.Client.Commands;
using IbraHabra.NET.Application.UseCases.Client.Queries;
using IbraHabra.NET.Domain.Constants.ValueObject;
using IbraHabra.NET.Domain.Contract;
using IbraHabra.NET.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using Wolverine;

namespace IbraHabra.NET.Adapter.Controller;

[ApiController]
[Route("api/admin/apps")]
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

    // Admin: Create application (reuses existing CreateClientCommand)
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClientCommand command)
    {
        var result = await _bus.InvokeAsync<ApiResult<CreateClientResponse>>(command);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetByClientId), new { clientId = result.Value!.ClientId }, result)
            : StatusCode(result.StatusCode, result);
    }

    // Admin: List applications with cursor-based pagination and optional filter by projectId
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] Guid? projectId, [FromQuery] string? cursor, [FromQuery] int pageSize = 20)
    {
        if (pageSize <= 0 || pageSize > 200)
            pageSize = 20;

        System.Linq.Expressions.Expression<Func<OauthApplication, bool>> predicate = a => true;
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

        var payload = new ListApplicationsResponse(items, nextCursor);
        var api = ApiResult<ListApplicationsResponse>.Ok(payload);
        return Ok(api);
    }

    // Admin: Get application by clientId (reuses existing GetClientByIdQuery)
    [HttpGet("{clientId}")]
    public async Task<IActionResult> GetByClientId([FromRoute] string clientId)
    {
        var result = await _bus.InvokeAsync<ApiResult<GetClientResponse>>(new GetClientByIdQuery(clientId));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    // Admin: Update application mutable properties
    [HttpPut("{clientId}")]
    public async Task<IActionResult> Update([FromRoute] string clientId, [FromBody] UpdateApplicationRequest request)
    {
        var app = await _appRepo.GetViaConditionAsync(a => a.ClientId == clientId);
        if (app == null)
            return StatusCode(404, ApiResult.Fail(404, "Client not found."));

        var descriptor = new OpenIddictApplicationDescriptor();
        await _appManager.PopulateAsync(descriptor, app);

        // Scalars - null means do not change, empty string is allowed to clear where applicable
        if (request.DisplayName is not null)
            descriptor.DisplayName = request.DisplayName;
        if (request.ApplicationType is not null)
            descriptor.ApplicationType = request.ApplicationType;
        if (request.ClientType is not null)
            descriptor.ClientType = request.ClientType;
        if (request.ConsentType is not null)
            descriptor.ConsentType = request.ConsentType;

        // Collections: null -> no change, empty list -> clear
        if (request.RedirectUris is not null)
        {
            descriptor.RedirectUris.Clear();
            foreach (var uriStr in request.RedirectUris)
            {
                if (!TryCreateUri(uriStr, out var uri))
                    return StatusCode(400, ApiResult.Fail(400, $"Invalid redirect URI: {uriStr}"));
                descriptor.RedirectUris.Add(uri!);
            }
        }

        if (request.PostLogoutRedirectUris is not null)
        {
            descriptor.PostLogoutRedirectUris.Clear();
            foreach (var uriStr in request.PostLogoutRedirectUris)
            {
                if (!TryCreateUri(uriStr, out var uri))
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
        await _appRepo.UpdateAsync(a => a.ClientId == clientId,
            a => a.SetProperty(x => x.UpdatedAt, DateTime.UtcNow));

        return Ok(ApiResult.Ok());
    }

    // Admin: Set active status
    [HttpPut("{clientId}/status")]
    public async Task<IActionResult> SetStatus([FromRoute] string clientId, [FromBody] SetStatusRequest request)
    {
        var updated = await _appRepo.UpdateAsync(
            a => a.ClientId == clientId,
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

        var newSecret = GenerateSecureSecret();

        var descriptor = new OpenIddictApplicationDescriptor();
        await _appManager.PopulateAsync(descriptor, app);
        descriptor.ClientSecret = newSecret;

        await _appManager.UpdateAsync(app, descriptor);
        await _appRepo.UpdateAsync(a => a.ClientId == clientId,
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

    private static bool TryCreateUri(string input, out Uri? uri)
    {
        if (Uri.TryCreate(input, UriKind.Absolute, out var created) &&
            (created.Scheme == Uri.UriSchemeHttp || created.Scheme == Uri.UriSchemeHttps))
        {
            uri = created;
            return true;
        }

        uri = null;
        return false;
    }

    private static string GenerateSecureSecret(int byteLength = 32)
    {
        var bytes = new byte[byteLength];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }

    // DTOs
    public record UpdateApplicationRequest(
        string? DisplayName,
        string? ApplicationType,
        string? ClientType,
        string? ConsentType,
        List<string>? RedirectUris,
        List<string>? PostLogoutRedirectUris,
        List<string>? Permissions);

    public record SetStatusRequest(bool IsActive);

    public record RotateSecretResponse(string ClientId, string NewClientSecret);

    public record AdminAppSummary(
        string Id,
        string ClientId,
        Guid ProjectId,
        string? DisplayName,
        string? ApplicationType,
        string? ClientType,
        string? ConsentType,
        bool IsActive,
        DateTime CreatedAt,
        DateTime? UpdatedAt);

    public record ListApplicationsResponse(IEnumerable<AdminAppSummary> Items, string? NextCursor);
}