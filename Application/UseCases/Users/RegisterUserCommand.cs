using IbraHabra.NET.Application.Dto.Response;
using IbraHabra.NET.Application.Helper;
using IbraHabra.NET.Domain.Entity;
using IbraHabra.NET.Domain.Interface;
using Microsoft.AspNetCore.Identity;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Users;

public record RegisterUserCommand(string Email, string Password, string? FirstName, string? LastName, string ClientId);

public record RegisterUserCommandResponse(Guid UserId);

public class RegisterUserHandler : IWolverineHandler
{
    public static async Task<ApiResult<RegisterUserCommandResponse>> Handle(
        RegisterUserCommand command,
        IRepo<OauthApplication, string> repo, // Note: OpenIddict uses string Id by default
        UserManager<User> userManager)
    {
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

        if (policy.RequireEmailVerification && !string.IsNullOrEmpty(command.Email))
        {
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            Console.WriteLine($"Confirmation token: {token}");
        }

        return ApiResult<RegisterUserCommandResponse>.Ok(new(user.Id));
    }
}