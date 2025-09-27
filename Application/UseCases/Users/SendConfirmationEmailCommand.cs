using IbraHabra.NET.Application.Dto.Response;
using IbraHabra.NET.Application.Services;
using IbraHabra.NET.Domain.Entity;
using IbraHabra.NET.Domain.Interface;
using Microsoft.AspNetCore.Identity;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Users;

public record SendConfirmationEmailCommand(string Email, string ClientId);

public class SendConfirmationEmailHandler : IWolverineHandler
{
    public static async Task<ApiResult> Handle(SendConfirmationEmailCommand command,
        IRepo<OauthApplication, string> repo, UserManager<User> userManager, IEmailService emailService)
    {
        if (string.IsNullOrEmpty(command.Email) || string.IsNullOrEmpty(command.ClientId))
            return ApiResult.Fail(400, "Email and ClientId are required.");

        var client = await repo.GetViaConditionAsync(c => c.ClientId == command.ClientId && c.IsActive);
        if (client == null)
            return ApiResult.Fail(404, "Client does not exist");

        var user = await userManager.FindByEmailAsync(command.Email);
        if (user == null)
            return ApiResult.Fail(404, "User not found.");

        if (user.EmailConfirmed)
            return ApiResult.Fail(400, "Email already confirmed.");

        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        // TODO: Send email
        var confirmationLink =
            $"{client.RedirectUris.First()}/confirm-email?email={command.Email}&token={Uri.EscapeDataString(token)}&clientId={command.ClientId}";
        await emailService.SendConfirmationEmailAsync(command.Email, confirmationLink);

        return ApiResult.Ok();
    }
}