using System.Text.Json;
using IbraHabra.NET.Application.Dto;
using IbraHabra.NET.Application.Dto.Response;
using IbraHabra.NET.Application.Utils;
using IbraHabra.NET.Domain.Constants;
using IbraHabra.NET.Domain.Contract;
using IbraHabra.NET.Domain.Entities;
using IbraHabra.NET.Infra.Persistent.Projections;
using Microsoft.AspNetCore.Identity;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Users.Commands.RegisterUser;

public record RegisterUserCommand(string Email, string Password, string? FirstName, string? LastName, string ClientId);

public record RegisterUserCommandResponse(Guid Id, bool RequiresEmailConfirmation);

public class RegisterUserHandler : IWolverineHandler
{
    public static async Task<ApiResult<RegisterUserCommandResponse>> Handle(
        RegisterUserCommand command,
        IRepo<OauthApplication, string> oauthAppRepo,
        UserManager<User> userManager,
        IPasswordHasher<User> passwordHasher
        // , IEmailService emailService
    )
    {
        var client = await oauthAppRepo.GetViaConditionAsync(
            c => c.ClientId == command.ClientId && c.IsActive,
            c => new AuthPolicyAndRedirectUriProjections(c.Properties, c.RedirectUris));

        if (client == null)
            return ApiResult<RegisterUserCommandResponse>.Fail(ApiErrors.OAuthApplication.NotFound());

        var policy = ReadAuthPolicy.GetAuthPolicy(client.Properties);
        var passwordValidation = ReadAuthPolicy.ValidatePasswordAgainstPolicy(command.Password, policy);

        if (!passwordValidation.isPassed)
            return ApiResult<RegisterUserCommandResponse>.Fail(
                ApiErrors.User.PasswordRequirementNotMet(passwordValidation.errorMsg!));

        var existingUser = await userManager.FindByEmailAsync(command.Email);
        if (existingUser != null)
            return ApiResult<RegisterUserCommandResponse>.Fail(ApiErrors.User.DuplicateEmail());

        var user = new User
        {
            UserName = command.Email,
            Email = command.Email,
            FirstName = command.FirstName,
            LastName = command.LastName,
            EmailConfirmed = !policy.RequireEmailVerification,
            SecurityStamp = Guid.NewGuid().ToString(),
            PasswordHash = passwordHasher.HashPassword(null!, command.Password)
        };

        var result = await userManager.CreateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return ApiResult<RegisterUserCommandResponse>.Fail(ApiErrors.User.FailToCreateUser(errors));
        }

        // 7. Handle email verification if required (fire-and-forget for better UX)
        // if (policy.RequireEmailVerification)
        // {
        //     // Use Task.Run to avoid DbContext issues in background task
        //     _ = Task.Run(async () =>
        //     {
        //         try
        //         {
        //             var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        //             var redirectUris = ParseRedirectUris(client.RedirectUris);
        //             var redirectUri = redirectUris.FirstOrDefault();
        //
        //             if (!string.IsNullOrEmpty(redirectUri))
        //             {
        //                 var confirmationLink = BuildConfirmationLink(redirectUri, command.Email, token, command.ClientId);
        //                 await emailService.SendConfirmationEmailAsync(command.Email, confirmationLink);
        //             }
        //         }
        //         catch
        //         {
        //             // Log error but don't fail registration
        //             // TODO: Add ILogger for production monitoring
        //         }
        //     });
        // }

        return ApiResult<RegisterUserCommandResponse>.Ok(
            new RegisterUserCommandResponse(user.Id, policy.RequireEmailVerification));
    }

    private static string BuildConfirmationLink(string redirectUri, string email, string token, string clientId)
    {
        redirectUri = redirectUri.TrimEnd('/');
        return
            $"{redirectUri}/confirm-email?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}&clientId={Uri.EscapeDataString(clientId)}";
    }

    private static List<string> ParseRedirectUris(string? redirectUrisJson)
    {
        if (string.IsNullOrEmpty(redirectUrisJson))
            return new List<string>();

        try
        {
            return JsonSerializer.Deserialize<List<string>>(redirectUrisJson) ?? new List<string>();
        }
        catch (JsonException)
        {
            return new List<string>();
        }
    }
}