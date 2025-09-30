using System.Text.Json;
using IbraHabra.NET.Application.Dto.Response;
using IbraHabra.NET.Application.Services;
using IbraHabra.NET.Domain.Entity;
using IbraHabra.NET.Domain.SharedKernel.Interface;
using Microsoft.AspNetCore.Identity;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Users.Commands.RegisterUser;

public record RegisterUserCommand(string Email, string Password, string? FirstName, string? LastName, string ClientId);

public record RegisterUserCommandResponse(Guid Id);

public class RegisterUserHandler : IWolverineHandler
{
    public static async Task<ApiResult<RegisterUserCommandResponse>> Handle(
        RegisterUserCommand command,
        IRepo<OauthApplication, string> repo,
        UserManager<User> userManager,
        IEmailService emailService)
    {
        if (string.IsNullOrEmpty(command.Email) || string.IsNullOrEmpty(command.Password))
            return ApiResult<RegisterUserCommandResponse>.Fail(400, "Email and Password are required.");
    
        var client = await repo.GetViaConditionAsync(c => c.ClientId == command.ClientId && c.IsActive);
        if (client == null)
            return ApiResult<RegisterUserCommandResponse>.Fail(400, "Invalid or inactive client.");

        if (await userManager.FindByEmailAsync(command.Email) != null)
            return ApiResult<RegisterUserCommandResponse>.Fail(409, "Email already registered");

        var policy = client.GetAuthPolicy();

        var user = new User
        {
            UserName = command.Email,
            Email = command.Email,
            FirstName = command.FirstName,
            LastName = command.LastName
        };

        var result = await userManager.CreateAsync(user, command.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return ApiResult<RegisterUserCommandResponse>.Fail(400, errors);
        }

        if (policy.RequireEmailVerification)
        {
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            
            var redirectUris = ParseRedirectUris(client.RedirectUris);
            var redirectUri = redirectUris.FirstOrDefault() ?? "https://yourdomain.com"; 
            
            var confirmationLink = $"{redirectUri}/confirm-email?email={command.Email}&token={Uri.EscapeDataString(token)}&clientId={command.ClientId}";
            await emailService.SendConfirmationEmailAsync(command.Email, confirmationLink);
        }

        return ApiResult<RegisterUserCommandResponse>.Ok(new(user.Id));
    }

    private static List<string> ParseRedirectUris(string? redirectUrisJson)
    {
        if (string.IsNullOrEmpty(redirectUrisJson))
            return new List<string>();

        try
        {
            var uris = JsonSerializer.Deserialize<List<string>>(redirectUrisJson);
            return uris ?? new List<string>();
        }
        catch (JsonException)
        {
            return new List<string>();
        }
    }
}