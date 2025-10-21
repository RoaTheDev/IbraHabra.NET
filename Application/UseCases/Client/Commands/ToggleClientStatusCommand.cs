using IbraHabra.NET.Application.Dto;
using IbraHabra.NET.Domain.Constants;
using IbraHabra.NET.Domain.Contract;
using IbraHabra.NET.Domain.Entities;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Client.Commands;

public record ToggleClientStatusCommand(
    string ClientId,
    bool IsActive);

public class ToggleClientStatusHandler : IWolverineHandler
{
    public static async Task<ApiResult<string>> Handle(
        ToggleClientStatusCommand command,
        IRepo<OauthApplication, string> appRepo)
    {
        var rowsAffected = await appRepo.UpdateAsync(
            command.ClientId, a => a
                .SetProperty(x => x.UpdatedAt, DateTime.UtcNow)
                .SetProperty(x => x.IsActive, command.IsActive)
        );

        if (rowsAffected == 0)
            return ApiResult<string>.Fail(ApiErrors.OAuthApplication.NotFound());

        var status = command.IsActive ? "activated" : "deactivated";
        return ApiResult<string>.Ok($"Client {status} successfully.");
    }
}