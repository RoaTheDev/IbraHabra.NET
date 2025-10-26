using IbraHabra.NET.Application.Dto;
using IbraHabra.NET.Domain.Constants;
using IbraHabra.NET.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Admin.Commands.CreateUser;

public record CreateUserCommand(
    string Email,
    string Password,
    string[] Roles,
    string? FirstName = null,
    string? LastName = null
);

public class CreateUserHandler : IWolverineHandler
{
    public static async Task<ApiResult> Handle(
        CreateUserCommand command,
        UserManager<User> userManager)
    {
        var existingUser = await userManager.FindByEmailAsync(command.Email);
        if (existingUser != null)
            return ApiResult.Fail(
                ApiErrors.User.DuplicateEmail());

        var user = new User
        {
            UserName = command.Email,
            Email = command.Email,
            FirstName = command.FirstName,
            LastName = command.LastName,
            EmailConfirmed = false,
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(user, command.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return ApiResult.Fail(
                ApiErrors.User.FailToCreateUser(errors));
        }

        if (command.Roles is { Length: > 0 })
        {
            await userManager.AddToRolesAsync(user, command.Roles);
        }

        return ApiResult.Ok();
    }
}