using FluentValidation;
using IbraHabra.NET.Application.Dto.Response;
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
        IRepo<OauthApplication, string> appRepo,
        IValidator<ToggleClientStatusCommand> validator)
    {
        var rowsAffected = await appRepo.UpdateAsync(
            command.ClientId, a => a
                .SetProperty(x => x.UpdatedAt, DateTime.UtcNow)
                .SetProperty(x => x.IsActive, command.IsActive)
        );

        if (rowsAffected == 0)
            return ApiResult<string>.Fail(404, "client not found");

        var status = command.IsActive ? "activated" : "deactivated";
        return ApiResult<string>.Ok($"Client {status} successfully.");
    }
}