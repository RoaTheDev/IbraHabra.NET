using IbraHabra.NET.Application.Dto;
using IbraHabra.NET.Application.Utils;
using IbraHabra.NET.Domain.Constants;
using IbraHabra.NET.Domain.Contract;
using IbraHabra.NET.Domain.Entities;
using OpenIddict.Abstractions;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Client.Commands;

public record RotateSecretCommand(string ClientId);

public record RotateSecretResponse(string ClientId, string NewClientSecret);

public class RotateSecretHandler : IWolverineHandler
{
    public async Task<ApiResult<RotateSecretResponse>> Handle(RotateSecretCommand command,
        IRepo<OauthApplication, string> appRepo, IOpenIddictApplicationManager appManager)
    {
        var app = await appRepo.GetViaConditionAsync(a => a.ClientId == command.ClientId);
        if (app == null)
            return ApiResult<RotateSecretResponse>.Fail(ApiErrors.OAuthApplication.NotFound());

        var newSecret = AuthUtils.GenerateSecureSecret();

        var descriptor = new OpenIddictApplicationDescriptor();
        await appManager.PopulateAsync(descriptor, app);
        descriptor.ClientSecret = newSecret;

        await appManager.UpdateAsync(app, descriptor);
        await appRepo.UpdateAsync(app.Id,
            a => a.SetProperty(x => x.UpdatedAt, DateTime.UtcNow));

        return ApiResult<RotateSecretResponse>.Ok(new RotateSecretResponse(app.ClientId!, newSecret));
    }
}