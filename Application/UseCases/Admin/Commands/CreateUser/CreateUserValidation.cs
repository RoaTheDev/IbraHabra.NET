using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace IbraHabra.NET.Application.UseCases.Admin.Commands.CreateUser;


public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator(IOptions<IdentityOptions> identityOptions)
    {
        var passwordOptions = identityOptions.Value.Password;

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Invalid email format")
            .MaximumLength(256)
            .WithMessage("Email must not exceed 256 characters");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .MinimumLength(passwordOptions.RequiredLength)
            .WithMessage($"Password must be at least {passwordOptions.RequiredLength} characters long")
            .MaximumLength(100)
            .WithMessage("Password must not exceed 100 characters");

        // Apply dynamic password rules based on Identity configuration
        if (passwordOptions.RequireUppercase)
        {
            RuleFor(x => x.Password)
                .Matches(@"[A-Z]")
                .WithMessage("Password must contain at least one uppercase letter");
        }

        if (passwordOptions.RequireLowercase)
        {
            RuleFor(x => x.Password)
                .Matches(@"[a-z]")
                .WithMessage("Password must contain at least one lowercase letter");
        }

        if (passwordOptions.RequireDigit)
        {
            RuleFor(x => x.Password)
                .Matches(@"[0-9]")
                .WithMessage("Password must contain at least one digit");
        }

        if (passwordOptions.RequireNonAlphanumeric)
        {
            RuleFor(x => x.Password)
                .Matches(@"[\W_]")
                .WithMessage("Password must contain at least one special character");
        }

        if (passwordOptions.RequiredUniqueChars > 1)
        {
            RuleFor(x => x.Password)
                .Must(password => password?.Distinct().Count() >= passwordOptions.RequiredUniqueChars)
                .WithMessage($"Password must contain at least {passwordOptions.RequiredUniqueChars} unique characters");
        }

        RuleFor(x => x.FirstName)
            .MaximumLength(100)
            .WithMessage("First name must not exceed 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.FirstName));

        RuleFor(x => x.LastName)
            .MaximumLength(100)
            .WithMessage("Last name must not exceed 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.LastName));

        RuleFor(x => x.Roles)
            .Must(roles => roles == null || roles.All(r => !string.IsNullOrWhiteSpace(r)))
            .WithMessage("Role names cannot be empty")
            .Must(roles => roles == null || roles.Distinct().Count() == roles.Length)
            .WithMessage("Duplicate roles are not allowed")
            .When(x => x.Roles != null && x.Roles.Length > 0);

        RuleForEach(x => x.Roles)
            .MaximumLength(50)
            .WithMessage("Role name must not exceed 50 characters")
            .When(x => x.Roles != null && x.Roles.Length > 0);
    }
}